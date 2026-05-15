import { useState, FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { createAgentProperty, updateAgentProperty, uploadPropertyImages, deletePropertyImage, type Property } from '../../api/properties'

interface Props {
  initial?: Partial<Property>
  id?: number
}

const TYPES = [
  { value: 0, label: 'Квартира' },
  { value: 1, label: 'Дом' },
  { value: 2, label: 'Участок' },
  { value: 3, label: 'Коммерческое' },
  { value: 4, label: 'Гараж' },
  { value: 5, label: 'Таунхаус' },
]

export default function PropertyForm({ initial, id }: Props) {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: initial?.title ?? '',
    description: initial?.description ?? '',
    type: 0,
    price: initial?.price ?? '',
    area: initial?.area ?? '',
    rooms: initial?.rooms ?? '',
    floor: initial?.floor ?? '',
    totalFloors: initial?.totalFloors ?? '',
    address: initial?.address ?? '',
    city: initial?.city ?? '',
    district: initial?.district ?? '',
    postalCode: initial?.postalCode ?? '',
    features: initial?.features ?? '',
  })

  // Existing images loaded from the server
  const [existingImages, setExistingImages] = useState<string[]>(() => {
    if (!initial?.imageUrls) return []
    try { return JSON.parse(initial.imageUrls) } catch { return [] }
  })

  const [files, setFiles] = useState<File[]>([])
  const [previews, setPreviews] = useState<string[]>([])
  const [deletingUrl, setDeletingUrl] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }))

  const handleFiles = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files ?? [])
    setFiles(selected)
    setPreviews(selected.map(f => URL.createObjectURL(f)))
  }

  const handleDeleteExisting = async (url: string) => {
    if (!id) return
    setDeletingUrl(url)
    try {
      await deletePropertyImage(id, url)
      setExistingImages(imgs => imgs.filter(u => u !== url))
    } catch {
      setError('Не удалось удалить фото')
    } finally {
      setDeletingUrl(null)
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      let createdId = id
      if (id) {
        await updateAgentProperty(id, form)
      } else {
        const res = await createAgentProperty(form)
        createdId = res.data.id
      }
      if (files.length > 0 && createdId) {
        await uploadPropertyImages(createdId, files)
      }
      navigate('/agent/properties')
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } }).response?.data?.message
      setError(msg ?? 'Ошибка при сохранении')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page-container">
      <h1 className="section-title">{id ? 'Редактировать объект' : 'Добавить объект'}</h1>
      {!id && (
        <div className="alert alert-info mb-16">
          После сохранения объект будет отправлен администратору на модерацию
        </div>
      )}
      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="card mb-16">
          <div className="card-title mb-16">Основная информация</div>
          <div className="form-group">
            <label>Название объекта *</label>
            <input className="form-control" value={form.title} onChange={set('title')} required />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Тип *</label>
              <select className="form-control" value={form.type} onChange={set('type')}>
                {TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label>Цена (₽) *</label>
              <input className="form-control" type="number" value={form.price} onChange={set('price')} required min={1} />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Площадь (м²)</label>
              <input className="form-control" type="number" value={form.area} onChange={set('area')} />
            </div>
            <div className="form-group">
              <label>Комнат</label>
              <input className="form-control" type="number" value={form.rooms} onChange={set('rooms')} min={0} />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Этаж</label>
              <input className="form-control" type="number" value={form.floor} onChange={set('floor')} min={1} />
            </div>
            <div className="form-group">
              <label>Этажей в доме</label>
              <input className="form-control" type="number" value={form.totalFloors} onChange={set('totalFloors')} min={1} />
            </div>
          </div>
          <div className="form-group">
            <label>Описание</label>
            <textarea className="form-control" rows={4} value={form.description} onChange={set('description')} />
          </div>
        </div>

        <div className="card mb-16">
          <div className="card-title mb-16">Адрес</div>
          <div className="form-group">
            <label>Адрес *</label>
            <input className="form-control" value={form.address} onChange={set('address')} required />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Город</label>
              <input className="form-control" value={form.city} onChange={set('city')} />
            </div>
            <div className="form-group">
              <label>Район</label>
              <input className="form-control" value={form.district} onChange={set('district')} />
            </div>
          </div>
          <div className="form-group">
            <label>Индекс</label>
            <input className="form-control" value={form.postalCode} onChange={set('postalCode')} />
          </div>
        </div>

        <div className="card mb-16">
          <div className="card-title mb-16">
            Фотографии
            {existingImages.length > 0 && (
              <span className="text-muted" style={{ fontSize: '0.85rem', fontWeight: 400, marginLeft: 8 }}>
                {existingImages.length} загружено
              </span>
            )}
          </div>

          {/* Existing photos */}
          {existingImages.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <div className="text-muted mb-8" style={{ fontSize: '0.85rem' }}>Текущие фото (нажмите ✕ для удаления):</div>
              <div className="upload-preview">
                {existingImages.map((url) => (
                  <div key={url} className="upload-preview-item">
                    <img src={url} alt="" />
                    <button
                      type="button"
                      className="upload-preview-remove"
                      disabled={deletingUrl === url}
                      onClick={() => handleDeleteExisting(url)}
                      title="Удалить фото"
                    >
                      {deletingUrl === url ? '…' : '✕'}
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* New photos upload */}
          <label className="upload-zone">
            <input type="file" multiple accept="image/*" onChange={handleFiles} />
            <div>
              <div style={{ fontSize: '2rem', marginBottom: 8 }}>📷</div>
              <div>Нажмите или перетащите фото</div>
              <div className="text-muted mt-4">JPG, PNG, WebP — до 10 файлов</div>
            </div>
            {previews.length > 0 && (
              <div className="upload-preview" onClick={e => e.preventDefault()}>
                {previews.map((url, i) => (
                  <div key={i} className="upload-preview-item">
                    <img src={url} alt="" />
                  </div>
                ))}
              </div>
            )}
          </label>
        </div>

        <div className="flex gap-12">
          <button type="submit" className="btn btn-primary btn-lg" disabled={loading}>
            {loading ? 'Сохранение...' : (id ? 'Сохранить изменения' : 'Отправить на модерацию')}
          </button>
          <button type="button" className="btn btn-ghost" onClick={() => navigate('/agent/properties')}>
            Отмена
          </button>
        </div>
      </form>
    </div>
  )
}
