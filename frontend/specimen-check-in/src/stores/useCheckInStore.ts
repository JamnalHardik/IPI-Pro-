import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ManifestListItem, ManifestDetail, AddOffManifestRequest } from '@/types'
import * as api from '@/api/manifestApi'

export type TabName = 'Check-In' | 'Scan History' | 'Manifests' | 'Discrepancies'

interface TenantInfo {
  id: string
  name: string
}

export const useCheckInStore = defineStore('checkin', () => {
  const tenantId = ref('00000000-0000-0000-0000-000000000001')
  const userId = ref('tech1')

  const activeTab = ref<TabName>('Check-In')
  const tenants = ref<TenantInfo[]>([])
  const userMenuOpen = ref(false)

  const manifests = ref<ManifestListItem[]>([])
  const selectedManifest = ref<ManifestDetail | null>(null)
  const loading = ref(false)
  const manifestsLoading = ref(false)
  const error = ref<string | null>(null)

  const selectedManifestId = ref<string | null>(null)

  function initTenant() {
    api.setTenantHeaders(tenantId.value, userId.value)
  }

  function setActiveTab(tab: TabName) {
    activeTab.value = tab
  }

  async function setTenant(id: string) {
    tenantId.value = id
    userId.value = 'tech1'
    api.setTenantHeaders(id, 'tech1')
    selectedManifest.value = null
    selectedManifestId.value = null
    error.value = null
    userMenuOpen.value = false
    await fetchManifests()
  }

  function setUser(user: string) {
    userId.value = user
    api.setTenantHeaders(tenantId.value, user)
    userMenuOpen.value = false
  }

  async function fetchTenants() {
    try {
      tenants.value = await api.getTenants()
    } catch {
      // silent fallback
    }
  }

  const currentTenantName = computed(() =>
    tenants.value.find(t => t.id === tenantId.value)?.name ?? 'Central Lab'
  )

  async function fetchManifests(status?: string) {
    manifestsLoading.value = true
    error.value = null
    try {
      manifests.value = await api.getManifests(status)
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to load manifests'
    } finally {
      manifestsLoading.value = false
    }
  }

  async function selectManifest(id: string) {
    loading.value = true
    error.value = null
    selectedManifestId.value = id
    try {
      selectedManifest.value = await api.getManifest(id)
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to load manifest'
    } finally {
      loading.value = false
    }
  }

  async function receiveSpecimen(specimenId: string) {
    if (!selectedManifest.value) return
    loading.value = true
    error.value = null
    try {
      selectedManifest.value = await api.receiveSpecimen(selectedManifest.value.id, specimenId)
      await refreshManifestList()
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to receive specimen'
    } finally {
      loading.value = false
    }
  }

  async function flagSpecimen(specimenId: string) {
    if (!selectedManifest.value) return
    loading.value = true
    error.value = null
    try {
      selectedManifest.value = await api.flagSpecimen(selectedManifest.value.id, specimenId)
      await refreshManifestList()
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to flag specimen'
    } finally {
      loading.value = false
    }
  }

  async function addOffManifest(request: AddOffManifestRequest) {
    if (!selectedManifest.value) return
    loading.value = true
    error.value = null
    try {
      selectedManifest.value = await api.addOffManifestSpecimen(selectedManifest.value.id, request)
      await refreshManifestList()
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to add specimen'
    } finally {
      loading.value = false
    }
  }

  async function closeManifest() {
    if (!selectedManifest.value) return
    loading.value = true
    error.value = null
    try {
      selectedManifest.value = await api.closeManifest(selectedManifest.value.id)
      await refreshManifestList()
    } catch (e: any) {
      error.value = e?.response?.data?.detail || 'Failed to close manifest'
    } finally {
      loading.value = false
    }
  }

  async function refreshManifestList() {
    try {
      manifests.value = await api.getManifests()
    } catch {
      // silent fail for background list refresh
    }
  }

  const recentManifests = computed(() =>
    manifests.value.slice(0, 6)
  )

  return {
    tenantId,
    userId,
    activeTab,
    tenants,
    userMenuOpen,
    currentTenantName,
    manifests,
    selectedManifest,
    selectedManifestId,
    loading,
    manifestsLoading,
    error,
    recentManifests,
    initTenant,
    setActiveTab,
    setTenant,
    setUser,
    fetchTenants,
    fetchManifests,
    selectManifest,
    receiveSpecimen,
    flagSpecimen,
    addOffManifest,
    closeManifest,
  }
})
