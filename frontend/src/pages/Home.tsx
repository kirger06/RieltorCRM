import { useEffect, useState, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { getPublicProperties, getPublicCities, getPublicCompanies, type Property, type PropertyFilters } from '../api/properties'
import { PropertyCardImage } from '../components/PropertyCardImage'

const TYPES = [
  { value: '', label: 'Все типы' },
  { value: '0', label: 'Квартира' },
  { value: '1', label: 'Дом' },
  { value: '2', label: 'Участок' },
  { value: '3', label: 'Коммерческое' },
  { value: '4', label: 'Гараж' },
  { value: '5', label: 'Таунхаус' },
]

const TYPE_LABELS: Record<string, string> = {
  Apartment: 'Квартира',
  House: 'Дом',
  Land: 'Участок',
  Commercial: 'Коммерческое',
  Garage: 'Гараж',
  Townhouse: 'Таунхаус',
}

export default function Home() {
  const [properties, setProperties] = useState<Property[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [cities, setCities] = useState<string[]>([])
  const [companies, setCompanies] = useState<{ id: number; name: string }[]>([])
  const [page, setPage] = useState(1)
  const pageSize = 12

  const [filters, setFilters] = useState<PropertyFilters>({})
  const [searchInput, setSearchInput] = useState('')

  const load = useCallback(async (f: PropertyFilters, p: number) => {
    setLoading(true)
    try {
      const res = await getPublicProperties({ ...f, page: p, pageSize })
      setProperties(res.data.items)
      setTotal(res.data.total)
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    getPublicCities().then(r => setCities(r.data)).catch(() => {})
    getPublicCompanies().then(r => setCompanies(r.data)).catch(() => {})
  }, [])

  useEffect(() => {
    load(filters, page)
  }, [filters, page, load])

  const totalPages = Math.ceil(total / pageSize)

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setPage(1)
    setFilters(f => ({ ...f, search: searchInput || undefined }))
  }

  const handleFilter = (key: keyof PropertyFilters, value: string) => {
    setPage(1)
    if (key === 'companyId' || key === 'minPrice' || key === 'maxPrice' || key === 'minRooms') {
      setFilters(f => ({ ...f, [key]: value ? Number(value) : undefined }))
    } else {
      setFilters(f => ({ ...f, [key]: value || undefined }))
    }
  }

  return (
    <div>
      {/* Hero */}
      <div className="hero">
        <h1>Найдите своё идеальное жильё</h1>
        <p>Тысячи объектов недвижимости — квартиры, дома, коммерческие помещения</p>
        <form className="hero-search" onSubmit={handleSearch}>
          <input
            type="text"
            placeholder="Город, адрес или название..."
            value={searchInput}
            onChange={e => setSearchInput(e.target.value)}
          />
          <button type="submit" className="btn btn-secondary btn-lg">Найти</button>
        </form>
      </div>

      <div className="page-container">
        {/* Filters */}
        <div className="filters-bar">
          <div className="filter-group">
            <label>Город</label>
            <select value={filters.city ?? ''} onChange={e => handleFilter('city', e.target.value)}>
              <option value="">Все города</option>
              {cities.map(c => <option key={c} value={c}>{c}</option>)}
            </select>
          </div>
          <div className="filter-group">
            <label>Тип</label>
            <select value={filters.type ?? ''} onChange={e => handleFilter('type', e.target.value)}>
              {TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>
          <div className="filter-group">
            <label>Компания</label>
            <select value={filters.companyId?.toString() ?? ''} onChange={e => handleFilter('companyId', e.target.value)}>
              <option value="">Все компании</option>
              {companies.map(c => <option key={c.id} value={c.id.toString()}>{c.name}</option>)}
            </select>
          </div>
          <div className="filter-group">
            <label>Цена от</label>
            <input
              type="number"
              placeholder="0"
              value={filters.minPrice ?? ''}
              onChange={e => handleFilter('minPrice', e.target.value)}
            />
          </div>
          <div className="filter-group">
            <label>Цена до</label>
            <input
              type="number"
              placeholder="∞"
              value={filters.maxPrice ?? ''}
              onChange={e => handleFilter('maxPrice', e.target.value)}
            />
          </div>
          <div className="filter-group">
            <label>Комнат от</label>
            <input
              type="number"
              placeholder="1"
              min={1}
              value={filters.minRooms ?? ''}
              onChange={e => handleFilter('minRooms', e.target.value)}
            />
          </div>
          <button
            className="btn btn-ghost btn-sm"
            style={{ alignSelf: 'flex-end' }}
            onClick={() => { setFilters({}); setSearchInput(''); setPage(1) }}
          >
            Сбросить
          </button>
        </div>

        {/* Results */}
        <div className="flex-between mb-16">
          <p className="text-muted">Найдено: {total} объектов</p>
        </div>

        {loading ? (
          <div className="loading">Загрузка...</div>
        ) : properties.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🏠</div>
            <p className="empty-state-text">Объектов не найдено. Попробуйте изменить фильтры.</p>
          </div>
        ) : (
          <div className="property-grid">
            {properties.map(p => (
              <Link key={p.id} to={`/properties/${p.id}`} className="property-card">
                <PropertyCardImage imageUrls={p.imageUrls} title={p.title} />
                <div className="property-card-body">
                  <div className="property-card-price">{p.price.toLocaleString('ru-RU')} ₽</div>
                  <div className="property-card-meta">
                    {TYPE_LABELS[p.type] ?? p.type}
                    {p.rooms && <span>{p.rooms} комн.</span>}
                    {p.area && <span>{p.area} м²</span>}
                    {p.floor && p.totalFloors && <span>{p.floor}/{p.totalFloors} эт.</span>}
                  </div>
                  <div className="property-card-title">{p.title}</div>
                  <div className="property-card-address">📍 {p.address}{p.city ? `, ${p.city}` : ''}</div>
                </div>
              </Link>
            ))}
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="pagination">
            <button
              className="pagination-btn"
              disabled={page === 1}
              onClick={() => setPage(p => p - 1)}
            >
              ←
            </button>
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              const p = i + 1
              return (
                <button
                  key={p}
                  className={`pagination-btn${page === p ? ' active' : ''}`}
                  onClick={() => setPage(p)}
                >
                  {p}
                </button>
              )
            })}
            <button
              className="pagination-btn"
              disabled={page === totalPages}
              onClick={() => setPage(p => p + 1)}
            >
              →
            </button>
          </div>
        )}
      </div>
    </div>
  )
}
