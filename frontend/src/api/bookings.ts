import api from './client'

export interface Booking {
  id: number
  type: string
  status: string
  createdAt: string
  property?: {
    id: number
    title: string
    address: string
    city?: string
    price: number
    area?: number
    rooms?: number
    imageUrls?: string
    status: string
    agentName?: string
    agentPhone?: string
  }
}

export const getMyBookings = () =>
  api.get<Booking[]>('/client/bookings')

export const createBooking = (propertyId: number, type: number) =>
  api.post('/client/bookings', { propertyId, type })

export const cancelBooking = (id: number) =>
  api.delete(`/client/bookings/${id}`)
