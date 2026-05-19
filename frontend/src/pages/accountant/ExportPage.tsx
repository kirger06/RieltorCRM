import { useEffect, useState } from 'react'
import { exportBookings, getBookingsData } from '../../api/accountant'
import { StatusBadge } from '../../components/StatusBadge'

interface BookingRow {
  id: number
  type: string
  status: string
  date: string
  clientName: string
  clientEmail: string
  clientPhone: string
  propertyTitle: string
  propertyAddress: string
  price: number
  agentName: string
}

export default function ExportPage() {
  const [bookings, setBookings] = useState<BookingRow[]>([])
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  useEffect(() => {
    getBookingsData()
      .then(r => setBookings(r.data))
      .catch(() => setMsg({ type: 'error', text: 'Не удалось загрузить данные' }))
      .finally(() => setLoading(false))
  }, [])

  const handleExport = async () => {
    setExporting(true)
    setMsg(null)
    try {
      await exportBookings()
      setMsg({ type: 'success', text: 'Файл Excel успешно скачан' })
    } catch {
      setMsg({ type: 'error', text: 'Ошибка при экспорте' })
    } finally {
      setExporting(false)
    }
  }

  const handlePrint = () => {
    const totalRevenue = bookings
      .filter(b => b.type === 'Purchase' && b.status === 'Active')
      .reduce((sum, b) => sum + b.price, 0)

    const printDate = new Date().toLocaleDateString('ru-RU', {
      day: '2-digit', month: 'long', year: 'numeric'
    })

    const rows = bookings.map(b => `
      <tr>
        <td>${b.id}</td>
        <td>${b.date}</td>
        <td>${b.clientName}<br><span style="color:#5f6368;font-size:11px">${b.clientEmail}</span></td>
        <td>${b.clientPhone || '—'}</td>
        <td>${b.propertyTitle}<br><span style="color:#5f6368;font-size:11px">${b.propertyAddress}</span></td>
        <td>${b.price.toLocaleString('ru-RU')} ₽</td>
        <td><span class="badge type-${b.type.toLowerCase()}">${translateType(b.type)}</span></td>
        <td><span class="badge status-${b.status.toLowerCase()}">${translateStatus(b.status)}</span></td>
        <td>${b.agentName || '—'}</td>
      </tr>
    `).join('')

    const html = `<!DOCTYPE html>
<html lang="ru">
<head>
  <meta charset="utf-8">
  <title>Отчёт по бронированиям</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Segoe UI', Arial, sans-serif; color: #202124; background: #fff; font-size: 12px; }

    .header { background: #1a73e8; color: #fff; padding: 18px 28px; display: flex; align-items: center; justify-content: space-between; }
    .header-logo { font-size: 20px; font-weight: 800; letter-spacing: -.5px; }
    .header-logo span { font-weight: 400; opacity: .7; }
    .header-meta { font-size: 11px; text-align: right; opacity: .85; }

    .summary { display: flex; gap: 16px; padding: 16px 28px; background: #e8f0fe; border-bottom: 2px solid #1a73e8; }
    .summary-item { }
    .summary-label { font-size: 10px; font-weight: 700; color: #1558b0; text-transform: uppercase; letter-spacing: .05em; }
    .summary-value { font-size: 16px; font-weight: 800; color: #1a73e8; }

    .content { padding: 20px 28px; }
    h2 { font-size: 13px; font-weight: 700; color: #3c4043; margin-bottom: 12px; text-transform: uppercase; letter-spacing: .05em; }

    table { width: 100%; border-collapse: collapse; }
    th { background: #f1f3f4; padding: 8px 10px; text-align: left; font-size: 10px; font-weight: 700; color: #5f6368; text-transform: uppercase; letter-spacing: .04em; border-bottom: 2px solid #e8eaed; white-space: nowrap; }
    td { padding: 7px 10px; font-size: 11px; color: #3c4043; border-bottom: 1px solid #f1f3f4; vertical-align: top; }
    tr:nth-child(even) td { background: #f8f9fa; }

    .badge { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 10px; font-weight: 700; }
    .badge.type-purchase { background: #f3e5f5; color: #6a1b9a; }
    .badge.type-reserve  { background: #fff8e1; color: #f57c00; }
    .badge.status-active    { background: #e3f2fd; color: #1565c0; }
    .badge.status-completed { background: #e8f5e9; color: #2e7d32; }
    .badge.status-cancelled { background: #fafafa; color: #9e9e9e; }

    .footer { padding: 16px 28px; border-top: 1px solid #e8eaed; display: flex; justify-content: space-between; color: #9aa0a6; font-size: 10px; margin-top: 12px; }

    @media print {
      @page { margin: 0; size: A4 landscape; }
      body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    }
  </style>
</head>
<body>
  <div class="header">
    <div class="header-logo">RieltorCRM <span>/ Отчёт по бронированиям</span></div>
    <div class="header-meta">Дата формирования: ${printDate}<br>Всего записей: ${bookings.length}</div>
  </div>

  <div class="summary">
    <div class="summary-item">
      <div class="summary-label">Всего бронирований</div>
      <div class="summary-value">${bookings.length}</div>
    </div>
    <div class="summary-item">
      <div class="summary-label">Общая выручка от продаж</div>
      <div class="summary-value">${totalRevenue.toLocaleString('ru-RU')} ₽</div>
    </div>
    <div class="summary-item">
      <div class="summary-label">Продажи</div>
      <div class="summary-value">${bookings.filter(b => b.type === 'Purchase').length}</div>
    </div>
    <div class="summary-item">
      <div class="summary-label">Резервы</div>
      <div class="summary-value">${bookings.filter(b => b.type === 'Reserve').length}</div>
    </div>
  </div>

  <div class="content">
    <h2>Список бронирований</h2>
    <table>
      <thead>
        <tr>
          <th>#</th>
          <th>Дата</th>
          <th>Клиент</th>
          <th>Телефон</th>
          <th>Объект</th>
          <th>Цена</th>
          <th>Тип</th>
          <th>Статус</th>
          <th>Агент</th>
        </tr>
      </thead>
      <tbody>${rows}</tbody>
    </table>
  </div>

  <div class="footer">
    <span>RieltorCRM — система управления недвижимостью</span>
    <span>Документ сформирован автоматически · ${printDate}</span>
  </div>

  <script>window.onload = () => { window.print(); }</script>
</body>
</html>`

    const win = window.open('', '_blank', 'width=1100,height=700')
    if (!win) return
    win.document.write(html)
    win.document.close()
  }

  return (
    <div className="page-container">
      <h1 className="section-title">Экспорт данных</h1>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      <div style={{ display: 'flex', gap: 16, marginBottom: 24, flexWrap: 'wrap', alignItems: 'stretch' }}>
        <div className="card" style={{ flex: '1 1 220px', maxWidth: 320, display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div>
            <div className="card-title mb-8">Excel-таблица</div>
            <p style={{ color: 'var(--gray-600)', marginBottom: 20, fontSize: '0.88rem' }}>
              Все бронирования компании в формате .xlsx со стилизованными заголовками и форматированием цен.
            </p>
          </div>
          <button className="btn btn-primary btn-lg" onClick={handleExport} disabled={exporting || loading}>
            {exporting ? 'Подготовка...' : '📥 Скачать Excel (.xlsx)'}
          </button>
        </div>

        <div className="card" style={{ flex: '1 1 220px', maxWidth: 320, display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
          <div>
            <div className="card-title mb-8">Печать / PDF</div>
            <p style={{ color: 'var(--gray-600)', marginBottom: 20, fontSize: '0.88rem' }}>
              Открывает форматированный отчёт с логотипом и сводкой. Используйте «Сохранить как PDF» в диалоге печати.
            </p>
          </div>
          <button className="btn btn-outline btn-lg" onClick={handlePrint} disabled={loading || bookings.length === 0}>
            🖨️ Печать / Сохранить PDF
          </button>
        </div>
      </div>

      {loading ? (
        <div className="loading">Загрузка данных...</div>
      ) : bookings.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📋</div>
          <div className="empty-state-text">Бронирований пока нет</div>
        </div>
      ) : (
        <>
          <h3 style={{ marginBottom: 12, color: 'var(--gray-700)' }}>
            Предпросмотр — {bookings.length} записей
          </h3>
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Дата</th>
                  <th>Клиент</th>
                  <th>Телефон</th>
                  <th>Объект</th>
                  <th>Цена</th>
                  <th>Тип</th>
                  <th>Статус</th>
                  <th>Агент</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map(b => (
                  <tr key={b.id}>
                    <td style={{ color: 'var(--gray-400)', fontSize: '0.8rem' }}>{b.id}</td>
                    <td style={{ whiteSpace: 'nowrap' }}>{b.date}</td>
                    <td>
                      <div style={{ fontWeight: 600 }}>{b.clientName}</div>
                      <div style={{ fontSize: '0.78rem', color: 'var(--gray-400)' }}>{b.clientEmail}</div>
                    </td>
                    <td>{b.clientPhone || '—'}</td>
                    <td>
                      <div style={{ fontWeight: 600 }}>{b.propertyTitle}</div>
                      <div style={{ fontSize: '0.78rem', color: 'var(--gray-400)' }}>{b.propertyAddress}</div>
                    </td>
                    <td style={{ fontWeight: 700, whiteSpace: 'nowrap' }}>
                      {b.price.toLocaleString('ru-RU')} ₽
                    </td>
                    <td><StatusBadge value={b.type} /></td>
                    <td><StatusBadge value={b.status} /></td>
                    <td>{b.agentName || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}

function translateType(type: string): string {
  const map: Record<string, string> = { Purchase: 'Покупка', Reserve: 'Резерв' }
  return map[type] ?? type
}

function translateStatus(status: string): string {
  const map: Record<string, string> = {
    Active: 'Активно', Completed: 'Завершено', Cancelled: 'Отменено'
  }
  return map[status] ?? status
}
