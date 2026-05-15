import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getAgentProperty, type Property } from '../../api/properties'
import PropertyForm from './PropertyForm'

export default function EditProperty() {
  const { id } = useParams<{ id: string }>()
  const [property, setProperty] = useState<Property | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    getAgentProperty(+id)
      .then(r => setProperty(r.data))
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return <div className="loading">Загрузка...</div>
  if (!property) return <div className="page-container">Объект не найден</div>

  return <PropertyForm initial={property} id={property.id} />
}
