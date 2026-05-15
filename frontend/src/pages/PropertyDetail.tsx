import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getPublicProperty, type Property } from '../api/properties'
import { createBooking } from '../api/bookings'
import { useAuth } from '../context/AuthContext'
import { StatusBadge } from '../components/StatusBadge'

const TYPE_LABELS: Record<string, string> = {
  Apartment: 'Квартира', House: 'Дом', Land: 'Участок',
  Commercial: 'Коммерческое', Garage: 'Гараж', Townhouse: 'Таунхаус',
}

export default function PropertyDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user, isAuthenticated } = useAuth()
  const [property, setProperty] = useState<Property | null>(null)
  const [loading, setLoading] = useState(true)
  const [activeImg, setActiveImg] = useState(0)
  const [showModal, setShowModal] = useState<'purchase' | 'reserve' | null>(null)
  const [booking, setBooking] = useState<{ success?: string; error?: string } | null>(null)

  useEffect(() => {
    if (!id) return
    getPublicProperty(+id)
      .then(r => setProperty(r.data))
      .catch(() => navigate('/'))
      .finally(() => setLoading(false))
  }, [id, navigate])

  const images: string[] = property?.imageUrls ? JSON.parse(property.imageUrls) : []

  const handleBook = async (type: 'purchase' | 'reserve') => {
    if (!isAuthenticated) { navigate('/login'); return }
    if (user?.role !== 'Client') { setBooking({ error: 'Только клиенты могут бронировать объекты' }); setShowModal(null); return }
    try {
      await createBooking(+id!, type === 'purchase' ? 0 : 1)
      setBooking({ success: type === 'purchase' ? 'Поздравляем! Объект успешно куплен.' : 'Объект забронирован.' })
      setProperty(p => p ? { ...p, status: type === 'purchase' ? 'Sold' : 'Reserved' } : p)
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } }).response?.data?.message ?? 'Ошибка при создании бронирования'
      setBooking({ error: msg })
    }
    setShowModal(null)
  }

  if (loading) return <div className="loading">Загрузка...</div>
  if (!property) return null

  const isAvailable = property.status === 'Available'

  return (
    <div className="page-container">
      <button className="btn btn-ghost btn-sm mb-16" onClick={() => navigate(-1)}>← Назад</button>

      {booking?.success && <div className="alert alert-success">{booking.success}</div>}
      {booking?.error && <div className="alert alert-error">{booking.error}</div>}

      <div className="property-detail-grid">
        {/* Left column */}
        <div>
          {/* Gallery */}
          {images.length > 0 ? (
            <div className="gallery-container">
              <img className="gallery-main" src={images[activeImg]} alt={property.title} />
              {images.length > 1 && (
                <div className="gallery-thumbs">
                  {images.map((url, i) => (
                    <img
                      key={i}
                      className={`gallery-thumb${activeImg === i ? ' active' : ''}`}
                      src={url}
                      alt=""
                      onClick={() => setActiveImg(i)}
                    />
                  ))}
                </div>
              )}
            </div>
          ) : (
            <div style={{ height: 300, background: 'var(--gray-100)', borderRadius: 'var(--radius)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '4rem', marginBottom: 16 }}>🏠</div>
          )}

          {/* Specs */}
          <div className="card mb-16">
            <div className="card-header">
              <h2 style={{ fontSize: '1.4rem', fontWeight: 800 }}>{property.title}</h2>
              <StatusBadge value={property.status} />
            </div>
            <div style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--primary)', marginBottom: 16 }}>
              {property.price.toLocaleString('ru-RU')} ₽
            </div>

            <div className="property-specs">
              <div className="spec-item">
                <div className="spec-label">Тип</div>
                <div className="spec-value">{TYPE_LABELS[property.type] ?? property.type}</div>
              </div>
              {property.area && (
                <div className="spec-item">
                  <div className="spec-label">Площадь</div>
                  <div className="spec-value">{property.area} м²</div>
                </div>
              )}
              {property.rooms && (
                <div className="spec-item">
                  <div className="spec-label">Комнат</div>
                  <div className="spec-value">{property.rooms}</div>
                </div>
              )}
              {property.floor && (
                <div className="spec-item">
                  <div className="spec-label">Этаж</div>
                  <div className="spec-value">{property.floor}{property.totalFloors ? `/${property.totalFloors}` : ''}</div>
                </div>
              )}
            </div>

            <div className="divider" />

            <div style={{ marginBottom: 8 }}>
              <strong>Адрес:</strong> {property.address}{property.city ? `, ${property.city}` : ''}{property.district ? `, ${property.district}` : ''}
            </div>

            {property.description && (
              <>
                <div className="divider" />
                <p style={{ lineHeight: 1.7, color: 'var(--gray-700)' }}>{property.description}</p>
              </>
            )}
          </div>
        </div>

        {/* Right column — booking card */}
        <div>
          <div className="card" style={{ position: 'sticky', top: 80 }}>
            <h3 style={{ marginBottom: 16 }}>Действия с объектом</h3>

            {isAvailable ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                <button
                  className="btn btn-primary btn-lg w-full"
                  onClick={() => isAuthenticated ? setShowModal('purchase') : navigate('/login')}
                >
                  🏠 Купить
                </button>
                <button
                  className="btn btn-outline btn-lg w-full"
                  onClick={() => isAuthenticated ? setShowModal('reserve') : navigate('/login')}
                >
                  📋 Забронировать
                </button>
                {!isAuthenticated && (
                  <p className="text-muted" style={{ textAlign: 'center', fontSize: '0.85rem' }}>
                    Войдите, чтобы купить или забронировать
                  </p>
                )}
              </div>
            ) : (
              <div className="alert alert-info">
                Объект недоступен для бронирования
              </div>
            )}

            <div className="divider" />

            <div>
              <div style={{ fontWeight: 600, marginBottom: 8 }}>Агент</div>
              {property.agentName ? (
                <>
                  <div style={{ marginBottom: 4 }}>{property.agentName}</div>
                  {property.agentPhone && (
                    <a href={`tel:${property.agentPhone}`} className="btn btn-ghost btn-sm w-full mt-8">
                      📞 {property.agentPhone}
                    </a>
                  )}
                </>
              ) : (
                <span className="text-muted">Не назначен</span>
              )}
            </div>

            {property.companyName && (
              <>
                <div className="divider" />
                <div>
                  <div style={{ fontWeight: 600, marginBottom: 8 }}>🏢 Компания</div>
                  <div style={{ fontWeight: 700, marginBottom: 6 }}>{property.companyName}</div>
                  {property.companyPhone && (
                    <a
                      href={`tel:${property.companyPhone}`}
                      className="btn btn-ghost btn-sm w-full"
                      style={{ marginBottom: 6 }}
                    >
                      📞 {property.companyPhone}
                    </a>
                  )}
                  {property.companyAddress && (
                    <div style={{ fontSize: '0.82rem', color: 'var(--gray-600)' }}>
                      📍 {property.companyAddress}
                    </div>
                  )}
                </div>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Confirmation modals */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-title">
              {showModal === 'purchase' ? '🏠 Подтвердить покупку' : '📋 Подтвердить бронирование'}
            </div>
            <p style={{ color: 'var(--gray-600)', marginBottom: 8 }}>{property.title}</p>
            <p style={{ fontSize: '1.5rem', fontWeight: 800, color: 'var(--primary)' }}>
              {property.price.toLocaleString('ru-RU')} ₽
            </p>
            {showModal === 'purchase' && (
              <p className="text-muted mt-8">После подтверждения объект будет закреплён за вами как купленный.</p>
            )}
            {showModal === 'reserve' && (
              <p className="text-muted mt-8">Объект будет забронирован. Вы можете отменить бронь в любой момент.</p>
            )}
            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setShowModal(null)}>Отмена</button>
              <button
                className={`btn ${showModal === 'purchase' ? 'btn-primary' : 'btn-outline'}`}
                onClick={() => handleBook(showModal)}
              >
                Подтвердить
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
