import api from './client'

export const getPendingProperties = (params?: { page?: number; pageSize?: number }) =>
  api.get('/admin/properties/pending', { params })

export const approveProperty = (id: number) =>
  api.post(`/admin/properties/${id}/approve`)

export const rejectProperty = (id: number, reason: string) =>
  api.post(`/admin/properties/${id}/reject`, { reason })

export const getAdminUsers = (params?: { search?: string; page?: number; pageSize?: number }) =>
  api.get('/admin/users', { params })

export const toggleUserActive = (id: number) =>
  api.put(`/admin/users/${id}/toggle-active`)

export const getCompanies = () =>
  api.get('/admin/companies')

export const getCompanyStats = (id: number) =>
  api.get(`/admin/companies/${id}/stats`)

export const getCompanyClients = (id: number) =>
  api.get(`/admin/companies/${id}/clients`)

export const getCompanyBookings = (id: number, params?: { page?: number; pageSize?: number }) =>
  api.get(`/admin/companies/${id}/bookings`, { params })

export const createCompany = (data: { name: string; description?: string; phone?: string; address?: string }) =>
  api.post('/admin/companies', data)
