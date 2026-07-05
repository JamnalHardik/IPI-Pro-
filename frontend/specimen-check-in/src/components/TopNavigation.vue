<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useCheckInStore } from '@/stores/useCheckInStore'
import type { TabName } from '@/stores/useCheckInStore'

const store = useCheckInStore()

const tabs: { name: TabName; badge?: { text: string; style: string } }[] = [
  { name: 'Check-In' },
  { name: 'Scan History', badge: { text: '12', style: 'bg-gray-100 text-gray-600' } },
  { name: 'Manifests' },
  { name: 'Discrepancies', badge: { text: '5', style: 'bg-red-100 text-red-600' } },
]

function onDocumentClick(e: MouseEvent) {
  const target = e.target as HTMLElement
  if (!target.closest('[data-user-menu]')) {
    store.userMenuOpen = false
  }
}

onMounted(() => document.addEventListener('click', onDocumentClick))
onUnmounted(() => document.removeEventListener('click', onDocumentClick))
</script>

<template>
  <div>
    <header class="bg-navy-900 h-14 flex items-center justify-between px-6">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 bg-white rounded-md flex items-center justify-center">
          <span class="text-navy-900 font-bold text-sm tracking-tight">IP</span>
        </div>
        <span class="bg-navy-700 text-white text-xs font-semibold px-3 py-1 rounded">UAT</span>
        <div class="text-white text-sm">
          Mode: <span class="font-semibold">Check-In</span>
          <span class="mx-3 text-white/30">|</span>
          Location: <span class="font-semibold">{{ store.currentTenantName }} — Receiving</span>
        </div>
      </div>

      <div class="relative" data-user-menu>
        <button
          class="flex items-center gap-3 cursor-pointer"
          @click.stop="store.userMenuOpen = !store.userMenuOpen"
        >
          <span class="text-white text-sm">{{ store.userId === 'tech2' ? 'Lab Tech 2' : 'Lab Tech 1' }}</span>
          <div class="w-9 h-9 rounded-full bg-teal-600 flex items-center justify-center text-white text-xs font-bold">
            {{ store.userId === 'tech2' ? 'T2' : 'LT' }}
          </div>
        </button>

        <div
          v-if="store.userMenuOpen"
          class="absolute right-0 top-full mt-2 w-72 bg-white rounded-xl shadow-lg border border-gray-200 z-50 py-2"
        >
          <div class="px-4 py-2 border-b border-gray-100">
            <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Switch Tenant</p>
          </div>
          <div class="py-1">
            <button
              v-for="t in store.tenants"
              :key="t.id"
              :class="[
                'w-full text-left px-4 py-2.5 text-sm hover:bg-gray-50 flex items-center gap-3 transition-colors',
                store.tenantId === t.id ? 'bg-teal-50 text-teal-700 font-semibold' : 'text-gray-700'
              ]"
              @click="store.setTenant(t.id)"
            >
              <span class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold"
                :class="store.tenantId === t.id ? 'bg-teal-600 text-white' : 'bg-gray-200 text-gray-600'"
              >
                {{ t.name.charAt(0) }}
              </span>
              {{ t.name }}
              <span v-if="store.tenantId === t.id" class="ml-auto text-teal-600 text-xs">Active</span>
            </button>
          </div>

          <div class="border-t border-gray-100 mt-1 pt-1">
            <div class="px-4 py-2">
              <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Switch User</p>
              <div class="space-y-1">
                <button
                  :class="['w-full text-left px-3 py-2 text-sm rounded-lg transition-colors', store.userId === 'tech1' ? 'bg-gray-100 font-semibold' : 'hover:bg-gray-50']"
                  @click="store.setUser('tech1')"
                >
                  Lab Tech 1 (tech1)
                </button>
                <button
                  :class="['w-full text-left px-3 py-2 text-sm rounded-lg transition-colors', store.userId === 'tech2' ? 'bg-gray-100 font-semibold' : 'hover:bg-gray-50']"
                  @click="store.setUser('tech2')"
                >
                  Lab Tech 2 (tech2)
                </button>
              </div>
            </div>
          </div>

          <div class="border-t border-gray-100 mt-1 pt-2 px-4 pb-2">
            <p class="text-[11px] text-gray-400">
              Tenant isolation: only this tenant's data is visible.
              <span v-if="store.tenantId === '00000000-0000-0000-0000-000000000001'">Central Lab shown.</span>
              <span v-else>North Lab shown.</span>
            </p>
          </div>
        </div>
      </div>
    </header>

    <nav class="bg-white border-b border-gray-200 px-6 flex items-center h-12">
      <button
        v-for="tab in tabs"
        :key="tab.name"
        class="relative"
        @click="store.setActiveTab(tab.name)"
      >
        <div
          :class="[
            'px-4 py-3 text-sm font-medium transition-colors',
            store.activeTab === tab.name
              ? 'text-blue-600'
              : 'text-gray-500 hover:text-gray-700'
          ]"
        >
          {{ tab.name }}
          <span
            v-if="tab.badge"
            :class="['ml-1.5 text-xs px-1.5 py-0.5 rounded font-medium inline-block', tab.badge.style]"
          >
            {{ tab.badge.text }}
          </span>
        </div>
        <span
          v-if="store.activeTab === tab.name"
          class="absolute bottom-0 left-0 right-0 h-0.5 bg-blue-600 rounded-t"
        />
      </button>
    </nav>
  </div>
</template>
