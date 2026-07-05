import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import KpiCards from '@/components/KpiCards.vue'
import type { ManifestDetail } from '@/types'

const mockManifest: ManifestDetail = {
  id: '1',
  code: 'MF-001',
  status: 'Open',
  clinicName: 'Test Clinic',
  clinicLocation: 'Bay 1',
  sentAt: '2026-06-30T08:00:00Z',
  closedAt: null,
  totalExpected: 7,
  totalReceived: 5,
  totalPending: 1,
  totalFlagged: 1,
  isReconciled: false,
  specimens: [],
  discrepancies: [],
}

describe('KpiCards', () => {
  it('displays the four KPI values correctly', () => {
    const wrapper = mount(KpiCards, { props: { manifest: mockManifest } })
    expect(wrapper.text()).toContain('7')
    expect(wrapper.text()).toContain('5')
    expect(wrapper.text()).toContain('1')
    expect(wrapper.text()).toContain('1')
  })

  it('renders four card elements', () => {
    const wrapper = mount(KpiCards, { props: { manifest: mockManifest } })
    const cards = wrapper.findAll('.border')
    expect(cards).toHaveLength(4)
  })
})
