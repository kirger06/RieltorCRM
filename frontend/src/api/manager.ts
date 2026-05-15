import api from './client'

export const getMyCompany = () =>
  api.get('/manager/company')

export const getManagerStats = () =>
  api.get('/manager/stats')

export const getManagerClients = (search?: string) =>
  api.get('/manager/clients', { params: { search } })

export const getManagerBookings = (params?: { page?: number; pageSize?: number }) =>
  api.get('/manager/bookings', { params })

export const getCompanyUsers = () =>
  api.get('/manager/users')

export const assignAgent = (userId: number) =>
  api.post(`/manager/users/${userId}/assign-agent`)

export const assignAccountant = (userId: number) =>
  api.post(`/manager/users/${userId}/assign-accountant`)

export const removeFromCompany = (userId: number) =>
  api.delete(`/manager/users/${userId}/remove`)
