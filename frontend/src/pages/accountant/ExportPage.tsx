import { useState } from 'react'
import { exportBookings } from '../../api/accountant'

export default function ExportPage() {
  const [loading, setLoading] = useState(false)
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const handleExport = async () => {
    setLoading(true)
    setMsg(null)
    try {
      await exportBookings()
      setMsg({ type: 'success', text: 'Файл успешно скачан' })
    } catch {
      setMsg({ type: 'error', text: 'Ошибка при экспорте' })
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page-container">
      <h1 className="section-title">Экспорт данных</h1>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      <div className="card" style={{ maxWidth: 480 }}>
        <div className="card-title mb-8">Экспорт бронирований</div>
        <p style={{ color: 'var(--gray-600)', marginBottom: 20 }}>
          Скачать все бронирования компании в формате CSV. Файл содержит информацию о клиентах, объектах, суммах и датах.
        </p>
        <button className="btn btn-primary btn-lg" onClick={handleExport} disabled={loading}>
          {loading ? 'Подготовка...' : '📥 Скачать CSV'}
        </button>
      </div>
    </div>
  )
}
