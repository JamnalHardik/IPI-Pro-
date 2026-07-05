import axios from 'axios'
import type { ManifestListItem, ManifestDetail, AddOffManifestRequest } from '@/types'

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

export function setTenantHeaders(tenantId: string, userId: string) {
  api.defaults.headers.common['X-Tenant-Id'] = tenantId
  api.defaults.headers.common['X-User-Id'] = userId
}

export async function getManifests(status?: string): Promise<ManifestListItem[]> {
  const params = status ? { status } : {}
  const { data } = await api.get<ManifestListItem[]>('/manifests', { params })
  return data
}

export async function getManifest(id: string): Promise<ManifestDetail> {
  const { data } = await api.get<ManifestDetail>(`/manifests/${id}`)
  return data
}

export async function receiveSpecimen(manifestId: string, specimenId: string): Promise<ManifestDetail> {
  const { data } = await api.post<ManifestDetail>(`/manifests/${manifestId}/specimens/${specimenId}/receive`)
  return data
}

export async function flagSpecimen(manifestId: string, specimenId: string): Promise<ManifestDetail> {
  const { data } = await api.post<ManifestDetail>(`/manifests/${manifestId}/specimens/${specimenId}/flag`)
  return data
}

export async function addOffManifestSpecimen(manifestId: string, request: AddOffManifestRequest): Promise<ManifestDetail> {
  const { data } = await api.post<ManifestDetail>(`/manifests/${manifestId}/specimens`, request)
  return data
}

export async function closeManifest(manifestId: string): Promise<ManifestDetail> {
  const { data } = await api.post<ManifestDetail>(`/manifests/${manifestId}/close`)
  return data
}

interface TenantInfo {
  id: string
  name: string
}

export async function getTenants(): Promise<TenantInfo[]> {
  const { data } = await api.get<TenantInfo[]>('/tenants')
  return data
}
