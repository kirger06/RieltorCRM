import { useState } from 'react'

export function PropertyCardImage({ imageUrls, title }: { imageUrls?: string; title: string }) {
  const [error, setError] = useState(false)

  const urls: string[] = imageUrls ? JSON.parse(imageUrls) : []
  const first = urls[0]

  if (!first || error) {
    return (
      <div className="property-card-image-placeholder">🏠</div>
    )
  }

  return (
    <img
      className="property-card-image"
      src={first}
      alt={title}
      onError={() => setError(true)}
    />
  )
}
