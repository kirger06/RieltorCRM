import api from './client'

export const getAccountantStats = () =>
  api.get('/accountant/stats')

export const getBookingsData = () =>
  api.get('/accountant/bookings/data')

export const exportBookings = async () => {
  const res = await api.get('/accountant/export/bookings', { responseType: 'blob' })
  const url = window.URL.createObjectURL(new Blob([res.data]))
  const a = document.createElement('a')
  a.href = url
  a.download = `bookings_${new Date().toISOString().slice(0, 10)}.xlsx`
  document.body.appendChild(a)
  a.click()
  a.remove()
  window.URL.revokeObjectURL(url)
}
