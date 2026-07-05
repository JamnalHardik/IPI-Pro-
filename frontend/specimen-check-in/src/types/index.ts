export type ManifestStatus =
  | 'InTransit'
  | 'Open'
  | 'Closed'
  | 'ClosedWithDiscrepancy'

export type SpecimenStatus =
  | 'Pending'
  | 'Received'
  | 'Flagged'
  | 'Added'

export type DiscrepancyType = 'Missing' | 'OffManifest'
export type DiscrepancyStatus = 'Open' | 'Resolved'

export interface ManifestListItem {
  id: string
  code: string
  status: ManifestStatus
  clinicName: string
  clinicLocation: string
  sentAt: string
  totalExpected: number
  totalReceived: number
  totalFlagged: number
  isReconciled: boolean
}

export interface SpecimenDto {
  id: string
  code: string
  patient: string
  site: string
  provider: string
  status: SpecimenStatus
  receivedBy: string | null
  receivedAt: string | null
}

export interface DiscrepancyDto {
  id: string
  type: DiscrepancyType
  note: string
  status: DiscrepancyStatus
  createdAt: string
  specimenCode: string | null
}

export interface ManifestDetail {
  id: string
  code: string
  status: ManifestStatus
  clinicName: string
  clinicLocation: string
  sentAt: string
  closedAt: string | null
  totalExpected: number
  totalReceived: number
  totalPending: number
  totalFlagged: number
  isReconciled: boolean
  specimens: SpecimenDto[]
  discrepancies: DiscrepancyDto[]
}

export interface AddOffManifestRequest {
  code: string
  patient: string
  site: string
  provider: string
  note?: string
}

export interface ProblemDetails {
  type: string
  title: string
  status: number
  detail: string
}
