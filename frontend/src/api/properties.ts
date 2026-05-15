import api from './client'
import axios from 'axios'

export interface Property {
  id: number
  title: string
  description?: string
  type: string
  status: string
  price: number
  area?: number
  rooms?: number
  floor?: number
  totalFloors?: number
  address: string
  city?: string
  district?: string
  postalCode?: string
  latitude?: number
  longitude?: number
  features?: string
  imageUrls?: string
  rejectionReason?: string
  agentName?: string
  agentPhone?: string
  companyId?: number
  companyName?: string
  companyPhone?: string
  companyAddress?: string
  createdAt: string
  updatedAt?: string
}

export interface PropertyListResult {
  total: number
  page: number
  pageSize: number
  items: Property[]
}

export interface PropertyFilters {
  city?: string
  type?: string
  minPrice?: number
  maxPrice?: number
  minRooms?: number
  maxRooms?: number
  search?: string
  page?: number
  pageSize?: number
  companyId?: number
}

// Public (no auth required)
const publicAxios = axios.create({ baseURL: '/api' })

export const getPublicProperties = (filters: PropertyFilters = {}) =>
  publicAxios.get<PropertyListResult>('/public/properties', { params: filters })

export const getPublicProperty = (id: number) =>
  publicAxios.get<Property>(`/public/properties/${id}`)

export const getPublicCities = () =>
  publicAxios.get<string[]>('/public/cities')

export const getPublicCompanies = () =>
  publicAxios.get<{ id: number; name: string }[]>('/public/companies')

export const getManagerProperties = (params?: { status?: string; page?: number; pageSize?: number }) =>
  api.get<PropertyListResult>('/manager/properties', { params })

// Agent
export const getAgentProperties = (params?: { status?: string; page?: number; pageSize?: number }) =>
  api.get<PropertyListResult>('/agent/properties', { params })

export const getAgentProperty = (id: number) =>
  api.get<Property>(`/agent/properties/${id}`)

export const createAgentProperty = (data: object) =>
  api.post('/agent/properties', data)

export const updateAgentProperty = (id: number, data: object) =>
  api.put(`/agent/properties/${id}`, data)

export const updateAgentPropertyStatus = (id: number, status: number) =>
  api.put(`/agent/properties/${id}/status`, { status })

export const uploadPropertyImages = (id: number, files: File[]) => {
  const form = new FormData()
  files.forEach(f => form.append('files', f))
  return api.post<{ urls: string[] }>(`/agent/properties/${id}/images`, form, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

export const deletePropertyImage = (id: number, url: string) =>
  api.delete(`/agent/properties/${id}/images`, { data: { url } })

export const deleteAgentProperty = (id: number) =>
  api.delete(`/agent/properties/${id}`)

export const getAgentDashboard = () =>
  api.get('/agent/dashboard')
