const STATUS_LABELS: Record<string, string> = {
  Available: 'Доступно',
  Reserved: 'Забронировано',
  Sold: 'Продано',
  Rented: 'Арендовано',
  UnderContract: 'Под договором',
  Inactive: 'Неактивно',
  PendingApproval: 'На модерации',
  ApprovalRejected: 'Отклонено',
  Active: 'Активно',
  Cancelled: 'Отменено',
  Completed: 'Завершено',
  Purchase: 'Покупка',
  Reserve: 'Бронь',
  Apartment: 'Квартира',
  House: 'Дом',
  Land: 'Участок',
  Commercial: 'Коммерческое',
  Garage: 'Гараж',
  Townhouse: 'Таунхаус',
}

const STATUS_CLASS: Record<string, string> = {
  Available: 'badge-available',
  Reserved: 'badge-reserved',
  Sold: 'badge-sold',
  Rented: 'badge-rented',
  PendingApproval: 'badge-pending',
  ApprovalRejected: 'badge-rejected',
  Active: 'badge-active',
  Cancelled: 'badge-cancelled',
  Completed: 'badge-completed',
  Purchase: 'badge-purchase',
}

export function StatusBadge({ value }: { value: string }) {
  const cls = STATUS_CLASS[value] ?? 'badge-active'
  const label = STATUS_LABELS[value] ?? value
  return <span className={`badge ${cls}`}>{label}</span>
}

export function formatStatus(value: string) {
  return STATUS_LABELS[value] ?? value
}
