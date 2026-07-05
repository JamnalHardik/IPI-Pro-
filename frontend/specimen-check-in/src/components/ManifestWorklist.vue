<script setup lang="ts">
import { ref, computed } from 'vue'
import { useCheckInStore } from '@/stores/useCheckInStore'
import StatusPill from '@/components/StatusPill.vue'

const store = useCheckInStore()
const searchQuery = ref('')
const verificationMode = ref<'fast' | 'full'>('fast')
const bottleCount = ref(0)

const expectedCount = computed(() => {
  if (store.selectedManifest) return store.selectedManifest.totalExpected
  return 0
})

const countStatus = computed(() => {
  if (bottleCount.value === 0) return 'neutral'
  if (bottleCount.value === expectedCount.value) return 'match'
  return 'nomatch'
})

function incrementCount() {
  if (bottleCount.value < 999) bottleCount.value++
}

function decrementCount() {
  if (bottleCount.value > 0) bottleCount.value--
}

function getReceivedText(m: { totalReceived: number, totalExpected: number }) {
  return `${m.totalReceived} / ${m.totalExpected} received`
}
</script>

<template>
  <aside class="w-80 bg-gray-50 border-r border-gray-200 flex flex-col overflow-y-auto">
    <div class="p-5 border-b border-gray-200 bg-white">
      <div class="flex items-center gap-2 mb-3">
        <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Verification workflow</h3>
        <span class="text-[10px] font-bold text-amber-600 bg-amber-50 border border-amber-200 px-1.5 py-0.5 rounded uppercase">
          Lab Setting
        </span>
      </div>
      <div class="flex rounded-lg overflow-hidden border border-gray-200">
        <button
          :class="[
            'flex-1 text-sm py-2 font-medium transition-colors',
            verificationMode === 'fast'
              ? 'bg-teal-600 text-white'
              : 'bg-white text-gray-600 hover:bg-gray-50'
          ]"
          @click="verificationMode = 'fast'"
        >
          Fast Count
        </button>
        <button
          :class="[
            'flex-1 text-sm py-2 font-medium transition-colors',
            verificationMode === 'full'
              ? 'bg-teal-600 text-white'
              : 'bg-white text-gray-600 hover:bg-gray-50'
          ]"
          @click="verificationMode = 'full'"
        >
          Full Scan
        </button>
      </div>
    </div>

    <div class="p-5 border-b border-gray-200 bg-white">
      <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Find Manifest</h3>
      <div class="relative">
        <div class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 4v1m0 11v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
          </svg>
        </div>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Scan or search manifest..."
          class="w-full text-sm pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg focus:ring-2 focus:ring-teal-500 focus:border-transparent outline-none bg-gray-50 placeholder:text-gray-400"
        />
      </div>
      <div v-if="searchQuery" class="mt-3 space-y-1">
        <button
          v-for="m in store.manifests.filter(x => x.code.toLowerCase().includes(searchQuery.toLowerCase()))"
          :key="m.id"
          class="w-full text-left p-2.5 hover:bg-gray-100 rounded-lg text-sm border border-gray-100 bg-white"
          @click="store.selectManifest(m.id); searchQuery = ''"
        >
          <div class="flex items-center justify-between">
            <span class="font-semibold text-gray-800">{{ m.code }}</span>
            <StatusPill :status="m.status" />
          </div>
        </button>
      </div>
    </div>

    <div class="p-5 border-b border-gray-200 bg-white">
      <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Verify &amp; Receive</h3>
      <p class="text-xs text-gray-500 mb-2">Total bottles counted by lab tech</p>
      <div class="flex items-center justify-center gap-3">
        <button
          @click="decrementCount"
          class="w-10 h-10 rounded-lg border border-gray-200 flex items-center justify-center text-gray-500 hover:bg-gray-50 text-lg font-medium transition-colors"
        >
          −
        </button>
        <div class="w-16 text-center text-2xl font-bold text-gray-800">
          {{ bottleCount }}
        </div>
        <button
          @click="incrementCount"
          class="w-10 h-10 rounded-lg border border-gray-200 flex items-center justify-center text-gray-500 hover:bg-gray-50 text-lg font-medium transition-colors"
        >
          +
        </button>
      </div>
      <div class="mt-3 text-center">
        <template v-if="countStatus === 'match'">
          <p class="text-sm font-semibold text-green-600">Matches {{ expectedCount }} expected — ready to close.</p>
        </template>
        <template v-else-if="countStatus === 'nomatch'">
          <p class="text-sm text-amber-600">
            {{ bottleCount }} counted — {{ expectedCount > 0 ? `${expectedCount} expected` : 'select a manifest' }}
          </p>
        </template>
        <template v-else>
          <p class="text-sm text-gray-400">{{ expectedCount > 0 ? `${expectedCount} expected` : 'Select a manifest' }}</p>
        </template>
      </div>
    </div>

    <div class="p-5 flex-1 bg-white">
      <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Recent Manifests</h3>
      <div v-if="store.manifestsLoading" class="text-sm text-gray-400 py-4 text-center">
        Loading...
      </div>
      <ul v-else class="space-y-2">
        <li
          v-for="m in store.recentManifests"
          :key="m.id"
          :class="[
            'p-3 rounded-lg cursor-pointer transition-all border',
            store.selectedManifestId === m.id
              ? 'bg-teal-50 border-teal-200 shadow-sm'
              : 'bg-white border-gray-100 hover:border-gray-200 hover:shadow-sm'
          ]"
          @click="store.selectManifest(m.id)"
        >
          <div class="flex items-start justify-between mb-1">
            <div>
              <span class="font-bold text-sm text-gray-800">{{ m.code }}</span>
              <p class="text-xs text-gray-500 mt-0.5">{{ m.clinicName }} — {{ m.clinicLocation || 'Main' }}</p>
            </div>
            <StatusPill :status="m.status" />
          </div>
          <div class="text-xs text-gray-500 mt-1">
            {{ getReceivedText(m) }}
          </div>
        </li>
      </ul>

      <div v-if="!store.manifestsLoading && store.recentManifests.length === 0" class="text-sm text-gray-400 py-8 text-center">
        No manifests found
      </div>

      <button
        class="w-full mt-4 py-2.5 text-sm text-gray-600 hover:text-gray-800 hover:bg-gray-50 rounded-lg border border-gray-200 flex items-center justify-center gap-1 transition-colors"
      >
        View all manifests
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
    </div>
  </aside>
</template>
