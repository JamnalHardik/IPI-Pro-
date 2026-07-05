<script setup lang="ts">
import { onMounted } from 'vue'
import { useCheckInStore } from '@/stores/useCheckInStore'
import TopNavigation from '@/components/TopNavigation.vue'
import ManifestWorklist from '@/components/ManifestWorklist.vue'
import ManifestDetail from '@/components/ManifestDetail.vue'

const store = useCheckInStore()

onMounted(() => {
  store.initTenant()
  store.fetchTenants()
  store.fetchManifests()
})
</script>

<template>
  <div class="min-h-screen flex flex-col bg-white">
    <TopNavigation />
    <div
      v-if="store.error && !store.selectedManifest"
      class="bg-red-50 border-b border-red-200 text-red-700 text-sm px-6 py-3 flex items-center justify-between"
    >
      <div class="flex items-center gap-2">
        <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <span>{{ store.error }}</span>
      </div>
      <div class="flex items-center gap-3">
        <button
          @click="store.fetchManifests()"
          class="text-red-600 hover:text-red-800 underline text-xs font-medium"
        >
          Retry
        </button>
        <button
          @click="store.error = null"
          class="text-red-400 hover:text-red-600 text-xs"
        >
          Dismiss
        </button>
      </div>
    </div>
    <div v-if="store.activeTab === 'Check-In'" class="flex flex-1 overflow-hidden">
      <ManifestWorklist />
      <ManifestDetail />
    </div>
    <div v-else class="flex-1 flex items-center justify-center p-12 bg-white">
      <div class="text-center max-w-md">
        <div class="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path v-if="store.activeTab === 'Scan History'" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            <path v-else-if="store.activeTab === 'Manifests'" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 012-2h2a2 2 0 012 2M9 5h6" />
            <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 class="text-lg font-medium text-gray-700 mb-2">{{ store.activeTab }}</h3>
        <p class="text-sm text-gray-500 mb-6">
          <template v-if="store.activeTab === 'Scan History'">Scan history and audit trail will appear here.</template>
          <template v-else-if="store.activeTab === 'Manifests'">Full manifest management view will appear here.</template>
          <template v-else>Discrepancy tracking and resolution will appear here.</template>
        </p>
        <button
          class="text-sm text-blue-600 hover:text-blue-700 font-medium"
          @click="store.setActiveTab('Check-In')"
        >
          ← Back to Check-In
        </button>
      </div>
    </div>
  </div>
</template>
