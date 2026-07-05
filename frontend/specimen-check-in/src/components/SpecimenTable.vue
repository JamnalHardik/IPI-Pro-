<script setup lang="ts">
import type { ManifestDetail, SpecimenDto } from '@/types'
import { useCheckInStore } from '@/stores/useCheckInStore'
import StatusPill from '@/components/StatusPill.vue'

defineProps<{
  manifest: ManifestDetail
}>()

const store = useCheckInStore()

function handleReceive(specimen: SpecimenDto) {
  store.receiveSpecimen(specimen.id)
}

function handleFlag(specimen: SpecimenDto) {
  store.flagSpecimen(specimen.id)
}

function canAct(specimen: SpecimenDto): boolean {
  return specimen.status === 'Pending'
}

function formatTime(iso: string | null): string {
  if (!iso) return '—'
  return new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
}
</script>

<template>
  <div class="mt-5">
    <div class="flex items-center justify-between mb-3">
      <h3 class="text-sm font-semibold text-gray-800">Specimens on manifest</h3>
      <span class="text-xs font-semibold text-teal-700 bg-teal-50 border border-teal-200 px-2.5 py-1 rounded">
        {{ manifest.totalReceived }} received
      </span>
    </div>

    <div class="bg-white border border-gray-200 rounded-lg overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-50 border-b border-gray-200">
          <tr>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Status</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Specimen ID</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Patient</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Site</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Provider</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">Received By</th>
            <th class="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider">At</th>
            <th class="text-right px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wider w-24"></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="s in manifest.specimens"
            :key="s.id"
            class="border-b border-gray-100 hover:bg-gray-50/50 transition-colors"
          >
            <td class="px-5 py-3.5">
              <StatusPill :status="s.status" />
            </td>
            <td class="px-5 py-3.5 font-mono text-xs font-semibold text-gray-800">{{ s.code }}</td>
            <td class="px-5 py-3.5 text-gray-700">{{ s.patient }}</td>
            <td class="px-5 py-3.5 text-gray-600">{{ s.site }}</td>
            <td class="px-5 py-3.5 text-gray-600">{{ s.provider }}</td>
            <td class="px-5 py-3.5 text-gray-600">{{ s.receivedBy || '—' }}</td>
            <td class="px-5 py-3.5 text-xs text-gray-500">
              {{ formatTime(s.receivedAt) }}
            </td>
            <td class="px-5 py-3.5 text-right">
              <div class="flex items-center justify-end gap-1">
                <button
                  v-if="canAct(s)"
                  class="w-8 h-8 flex items-center justify-center rounded text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                  title="Edit specimen"
                  @click="handleReceive(s)"
                  :disabled="store.loading"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button
                  v-if="canAct(s)"
                  class="w-8 h-8 flex items-center justify-center rounded text-red-400 hover:text-red-600 hover:bg-red-50 transition-colors"
                  title="Flag as missing"
                  @click="handleFlag(s)"
                  :disabled="store.loading"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
                  </svg>
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="manifest.specimens.length === 0" class="p-8 text-center text-gray-400 text-sm">
        No specimens in this manifest
      </div>
    </div>
  </div>
</template>
