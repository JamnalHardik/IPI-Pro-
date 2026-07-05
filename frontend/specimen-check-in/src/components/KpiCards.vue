<script setup lang="ts">
import type { ManifestDetail } from '@/types'

defineProps<{
  manifest: ManifestDetail
}>()
</script>

<template>
  <div class="grid grid-cols-4 gap-4">
    <div class="bg-white border border-gray-200 rounded-lg p-5 text-center">
      <div class="text-3xl font-bold text-navy-800">{{ manifest.totalExpected }}</div>
      <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-1">Expected</div>
    </div>
    <div class="bg-white border border-gray-200 rounded-lg p-5 text-center">
      <div class="text-3xl font-bold text-teal-600">{{ manifest.totalReceived }}</div>
      <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-1">Received</div>
    </div>
    <div class="bg-white border border-gray-200 rounded-lg p-5 text-center">
      <div class="text-3xl font-bold text-navy-800">{{ manifest.totalPending }}</div>
      <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-1">Pending</div>
    </div>
    <div class="bg-white border border-gray-200 rounded-lg p-5 text-center">
      <div class="text-3xl font-bold text-red-500">{{ manifest.totalFlagged }}</div>
      <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-1">Flagged</div>
    </div>
  </div>
</template>
