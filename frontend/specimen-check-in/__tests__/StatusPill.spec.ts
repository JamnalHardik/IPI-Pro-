import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import StatusPill from '@/components/StatusPill.vue'

describe('StatusPill', () => {
  it('renders Pending with correct classes', () => {
    const wrapper = mount(StatusPill, { props: { status: 'Pending' } })
    expect(wrapper.text()).toBe('Pending')
    expect(wrapper.classes()).toContain('bg-amber-50')
  })

  it('renders Received with correct classes and checkmark icon', () => {
    const wrapper = mount(StatusPill, { props: { status: 'Received' } })
    expect(wrapper.text()).toContain('Received')
    expect(wrapper.classes()).toContain('bg-green-50')
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('renders Flagged with correct classes', () => {
    const wrapper = mount(StatusPill, { props: { status: 'Flagged' } })
    expect(wrapper.text()).toBe('Flagged')
    expect(wrapper.classes()).toContain('bg-red-50')
  })

  it('renders Added with correct classes', () => {
    const wrapper = mount(StatusPill, { props: { status: 'Added' } })
    expect(wrapper.text()).toBe('Added')
    expect(wrapper.classes()).toContain('bg-purple-50')
  })

  it('renders manifest status InTransit', () => {
    const wrapper = mount(StatusPill, { props: { status: 'InTransit' } })
    expect(wrapper.text()).toBe('In transit')
    expect(wrapper.classes()).toContain('bg-teal-50')
  })

  it('renders ClosedWithDiscrepancy as 1 discrepancy', () => {
    const wrapper = mount(StatusPill, { props: { status: 'ClosedWithDiscrepancy' } })
    expect(wrapper.text()).toBe('1 discrepancy')
  })
})
