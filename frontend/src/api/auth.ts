import api from './client'

export interface LoginDto {
  email: string
  password: string
}

export interface RegisterDto {
  email: string
  password: string
  firstName: string
  lastName: string
  middleName?: string
  phoneNumber?: string
  registerAsManager?: boolean
  companyName?: string
}

export interface UserInfo {
  id: number
  email: string
  firstName: string
  lastName: string
  middleName?: string
  phoneNumber?: string
  role: string
  companyId?: number
  createdAt: string
}

export const login = (data: LoginDto) =>
  api.post<{ token: string; user: UserInfo }>('/auth/login', data)

export const register = (data: RegisterDto) =>
  api.post<{ token: string; user: UserInfo }>('/auth/register', data)

export const getProfile = () =>
  api.get<UserInfo>('/auth/profile')
