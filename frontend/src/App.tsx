import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import Navbar from './components/Navbar'

// Public pages
import Home from './pages/Home'
import PropertyDetail from './pages/PropertyDetail'
import Login from './pages/Login'
import Register from './pages/Register'

// Auth-required (all roles)
import Dashboard from './pages/Dashboard'
import Profile from './pages/Profile'

// Client
import MyBookings from './pages/client/MyBookings'

// Agent
import MyProperties from './pages/agent/MyProperties'
import CreateProperty from './pages/agent/CreateProperty'
import EditProperty from './pages/agent/EditProperty'
import AgentDashboard from './pages/agent/AgentDashboard'

// Admin
import ModerationQueue from './pages/admin/ModerationQueue'
import UsersList from './pages/admin/UsersList'
import CompaniesList from './pages/admin/CompaniesList'
import CompanyView from './pages/admin/CompanyView'

// Manager
import ClientsList from './pages/manager/ClientsList'
import CompanyStats from './pages/manager/CompanyStats'
import ManagerBookings from './pages/manager/ManagerBookings'
import CompanyUsers from './pages/manager/CompanyUsers'
import ManagerProperties from './pages/manager/ManagerProperties'

// Accountant
import StatsView from './pages/accountant/StatsView'
import ExportPage from './pages/accountant/ExportPage'

// Layout with navbar for all pages (public + private)
function Layout() {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Navbar />
      <main style={{ flex: 1 }}>
        <Outlet />
      </main>
    </div>
  )
}

// Layout requiring auth
function AuthLayout() {
  return (
    <ProtectedRoute>
      <Layout />
    </ProtectedRoute>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public routes — show navbar but no auth required */}
          <Route element={<Layout />}>
            <Route path="/" element={<Home />} />
            <Route path="/properties/:id" element={<PropertyDetail />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
          </Route>

          {/* Auth-required routes */}
          <Route element={<AuthLayout />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/profile" element={<Profile />} />

            {/* Client */}
            <Route path="/client/bookings" element={<MyBookings />} />

            {/* Agent */}
            <Route path="/agent/properties" element={<MyProperties />} />
            <Route path="/agent/properties/create" element={<CreateProperty />} />
            <Route path="/agent/properties/:id/edit" element={<EditProperty />} />
            <Route path="/agent/dashboard" element={<AgentDashboard />} />

            {/* Admin */}
            <Route path="/admin/moderation" element={<ModerationQueue />} />
            <Route path="/admin/users" element={<UsersList />} />
            <Route path="/admin/companies" element={<CompaniesList />} />
            <Route path="/admin/companies/:id" element={<CompanyView />} />

            {/* Manager */}
            <Route path="/manager/clients" element={<ClientsList />} />
            <Route path="/manager/stats" element={<CompanyStats />} />
            <Route path="/manager/bookings" element={<ManagerBookings />} />
            <Route path="/manager/users" element={<CompanyUsers />} />
            <Route path="/manager/properties" element={<ManagerProperties />} />

            {/* Accountant */}
            <Route path="/accountant/stats" element={<StatsView />} />
            <Route path="/accountant/export" element={<ExportPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
