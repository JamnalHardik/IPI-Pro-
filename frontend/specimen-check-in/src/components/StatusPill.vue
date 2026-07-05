<script setup lang="ts">
import type { ManifestStatus, SpecimenStatus } from '@/types'

const props = defineProps<{
  status: ManifestStatus | SpecimenStatus
}>()

const colorMap: Record<string, { classes: string, icon: boolean }> = {
  InTransit: { classes: 'bg-teal-50 text-teal-700 border border-teal-200', icon: false },
  Open: { classes: 'bg-green-50 text-green-700 border border-green-200', icon: false },
  Closed: { classes: 'bg-gray-100 text-gray-600 border border-gray-200', icon: false },
  ClosedWithDiscrepancy: { classes: 'bg-red-50 text-red-600 border border-red-200', icon: false },
  Pending: { classes: 'bg-amber-50 text-amber-700 border border-amber-200', icon: false },
  Received: { classes: 'bg-green-50 text-green-700 border border-green-200', icon: true },
  Flagged: { classes: 'bg-red-50 text-red-600 border border-red-200', icon: false },
  Added: { classes: 'bg-purple-50 text-purple-700 border border-purple-200', icon: false },
}

const labelMap: Record<string, string> = {
  InTransit: 'In transit',
  Open: 'Open',
  Closed: 'Closed',
  ClosedWithDiscrepancy: '1 discrepancy',
  Pending: 'Pending',
  Received: 'Received',
  Flagged: 'Flagged',
  Added: 'Added',
}

</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-xs font-semibold whitespace-nowrap',
      (colorMap[status] ?? colorMap.Pending).classes
    ]"
  >
    <svg
      v-if="(colorMap[status] ?? colorMap.Pending).icon"
      class="w-3.5 h-3.5"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      stroke-width="2.5"
    >
      <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
    </svg>
    {{ labelMap[status] || status }}
  </span>
</template>
