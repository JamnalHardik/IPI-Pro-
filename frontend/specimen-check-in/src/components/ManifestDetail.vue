<script setup lang="ts">
import { ref } from 'vue'
import { useCheckInStore } from '@/stores/useCheckInStore'
import KpiCards from '@/components/KpiCards.vue'
import SpecimenTable from '@/components/SpecimenTable.vue'

const store = useCheckInStore()

const showAddForm = ref(false)
const addForm = ref({
  code: '',
  patient: '',
  site: '',
  provider: '',
  note: '',
})

function handleAddOffManifest() {
  if (!addForm.value.code || !addForm.value.patient) return
  store.addOffManifest({
    code: addForm.value.code,
    patient: addForm.value.patient,
    site: addForm.value.site || 'Unknown',
    provider: addForm.value.provider || 'Unknown',
    note: addForm.value.note || undefined,
  })
  showAddForm.value = false
  addForm.value = { code: '', patient: '', site: '', provider: '', note: '' }
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString('en-US', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}
</script>

<template>
  <main class="flex-1 overflow-y-auto p-6 bg-white">
    <template v-if="store.loading && !store.selectedManifest">
      <div class="flex items-center justify-center h-full">
        <div class="text-center">
          <div class="animate-spin w-8 h-8 border-2 border-teal-600 border-t-transparent rounded-full mx-auto mb-3"></div>
          <p class="text-sm text-gray-500">Loading manifest...</p>
        </div>
      </div>
    </template>

    <template v-else-if="!store.selectedManifest">
      <div class="flex items-center justify-center h-full">
        <div class="text-center max-w-md">
          <div class="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
            <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 012-2h2a2 2 0 012 2M9 5h6" />
            </svg>
          </div>
          <h3 class="text-lg font-medium text-gray-700 mb-2">Select a Manifest</h3>
          <p class="text-sm text-gray-500">
            Choose a manifest from the worklist on the left to begin the check-in process.
          </p>
        </div>
      </div>
    </template>

    <template v-else>
      <div class="max-w-5xl">
        <!-- Manifest Header -->
        <div class="flex items-start justify-between mb-5">
          <div>
            <div class="flex items-center gap-3 mb-1">
              <h2 class="text-xl font-bold text-gray-900">Manifest {{ store.selectedManifest.code }}</h2>
              <span class="px-2.5 py-0.5 rounded text-xs font-semibold bg-teal-100 text-teal-700">
                Fast Count
              </span>
            </div>
            <p class="text-sm text-gray-500 leading-relaxed">
              From {{ store.selectedManifest.clinicName }} — {{ store.selectedManifest.clinicLocation }}
              · Sent {{ formatDate(store.selectedManifest.sentAt) }}
              · {{ store.selectedManifest.totalExpected }} specimens expected
              <span v-if="store.selectedManifest.closedAt">
                · Closed {{ formatDate(store.selectedManifest.closedAt) }}
              </span>
            </p>
          </div>
          <div class="flex gap-3">
            <button
              @click="showAddForm = !showAddForm"
              class="text-sm border border-red-300 text-red-600 hover:bg-red-50 px-4 py-2 rounded-lg transition-colors flex items-center gap-1.5"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
              </svg>
              Flag discrepancy
            </button>
            <button
              :disabled="!store.selectedManifest.isReconciled || store.loading"
              @click="store.closeManifest()"
              class="text-sm bg-navy-900 hover:bg-navy-800 disabled:bg-gray-300 disabled:cursor-not-allowed text-white px-5 py-2 rounded-lg transition-colors font-medium"
            >
              Mark Received &amp; Close
            </button>
          </div>
        </div>

        <!-- Error banner -->
        <div v-if="store.error" class="bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg p-3 mb-4 flex items-center justify-between">
          <span>{{ store.error }}</span>
          <button @click="store.error = null" class="text-red-500 hover:text-red-700 text-xs underline">Dismiss</button>
        </div>

        <!-- Off-Manifest Add Form -->
        <div v-if="showAddForm" class="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-5">
          <h4 class="text-sm font-semibold text-amber-800 mb-3">Add Off-Manifest Specimen</h4>
          <div class="grid grid-cols-5 gap-3">
            <input v-model="addForm.code" placeholder="Specimen Code *" class="text-sm px-3 py-2 border border-amber-200 rounded-lg outline-none focus:ring-2 focus:ring-teal-500 bg-white" />
            <input v-model="addForm.patient" placeholder="Patient Name *" class="text-sm px-3 py-2 border border-amber-200 rounded-lg outline-none focus:ring-2 focus:ring-teal-500 bg-white" />
            <input v-model="addForm.site" placeholder="Body Site" class="text-sm px-3 py-2 border border-amber-200 rounded-lg outline-none focus:ring-2 focus:ring-teal-500 bg-white" />
            <input v-model="addForm.provider" placeholder="Provider" class="text-sm px-3 py-2 border border-amber-200 rounded-lg outline-none focus:ring-2 focus:ring-teal-500 bg-white" />
            <div class="flex gap-2">
              <button
                @click="handleAddOffManifest"
                class="flex-1 text-sm bg-amber-600 hover:bg-amber-700 text-white rounded-lg transition-colors font-medium"
              >
                Add
              </button>
              <button
                @click="showAddForm = false"
                class="text-sm border border-gray-300 text-gray-600 px-3 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>

        <!-- KPI Cards -->
        <KpiCards :manifest="store.selectedManifest" />

        <!-- Discrepancies -->
        <div v-if="store.selectedManifest.discrepancies.length > 0" class="mt-5">
          <h4 class="text-sm font-semibold text-gray-600 mb-2">
            Discrepancies ({{ store.selectedManifest.discrepancies.length }})
          </h4>
          <div class="space-y-2 mb-5">
            <div
              v-for="d in store.selectedManifest.discrepancies"
              :key="d.id"
              class="bg-orange-50 border border-orange-200 rounded-lg p-3 text-sm"
            >
              <span class="font-semibold text-orange-700 mr-2">{{ d.type === 'Missing' ? '[Missing]' : '[Off-Manifest]' }}</span>
              <span class="text-orange-800">{{ d.note }}</span>
              <span v-if="d.specimenCode" class="text-xs text-orange-500 ml-2">({{ d.specimenCode }})</span>
            </div>
          </div>
        </div>

        <!-- Specimen Table -->
        <SpecimenTable :manifest="store.selectedManifest" />
      </div>
    </template>
  </main>
</template>
