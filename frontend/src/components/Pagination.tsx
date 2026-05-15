interface PaginationProps {
  page: number
  total: number
  pageSize: number
  onPage: (p: number) => void
}

export default function Pagination({ page, total, pageSize, onPage }: PaginationProps) {
  const totalPages = Math.ceil(total / pageSize)
  if (totalPages <= 1) return null

  return (
    <div className="pagination">
      <button
        className="btn-secondary btn-sm"
        disabled={page <= 1}
        onClick={() => onPage(page - 1)}
      >
        ←
      </button>
      <span style={{ fontSize: 13, color: '#6b7280' }}>
        {page} / {totalPages} ({total} записей)
      </span>
      <button
        className="btn-secondary btn-sm"
        disabled={page >= totalPages}
        onClick={() => onPage(page + 1)}
      >
        →
      </button>
    </div>
  )
}
