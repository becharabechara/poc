import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Search, Plus, FileText, Tag, Home, Sparkles } from 'lucide-react';
import { cn } from '../utils';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const location = useLocation();
  const navigate = useNavigate();

  const navigation = [
    { name: 'Home', href: '/', icon: Home },
    { name: 'Search', href: '/search', icon: Search },
  ];

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="App">
      {/* Modern Header */}
      <header className="app-header">
        <div className="header-content">
          {/* Logo and Navigation */}
          <div className="flex items-center space-x-8">
            <Link to="/" className="logo">
              <div className="logo-icon">
                <Sparkles className="w-5 h-5" />
              </div>
              <span>SmartNotes</span>
            </Link>
            
            <nav className="nav-links hidden md:flex">
              {navigation.map((item) => {
                const Icon = item.icon;
                return (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={cn('nav-link', isActive(item.href) && 'active')}
                  >
                    <Icon className="w-4 h-4 mr-1" />
                    {item.name}
                  </Link>
                );
              })}
            </nav>
          </div>

          {/* Actions */}
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/create')}
              className="btn-modern"
            >
              <Plus className="w-4 h-4" />
              New Note
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="main-content">
        {children}
      </main>

      {/* Mobile Navigation */}
      <div className="md:hidden fixed bottom-0 left-0 right-0 bg-white/95 backdrop-blur-md border-t border-white/20 shadow-lg">
        <div className="grid grid-cols-3 gap-1 p-3">
          {navigation.map((item) => {
            const Icon = item.icon;
            return (
              <Link
                key={item.name}
                to={item.href}
                className={cn(
                  'flex flex-col items-center justify-center py-3 px-2 text-xs font-medium rounded-lg transition-all',
                  isActive(item.href)
                    ? 'text-blue-600 bg-blue-50 shadow-sm'
                    : 'text-gray-600 hover:text-blue-600 hover:bg-blue-50/50'
                )}
              >
                <Icon className="h-5 w-5 mb-1" />
                <span>{item.name}</span>
              </Link>
            );
          })}
          <button
            onClick={() => navigate('/create')}
            className="flex flex-col items-center justify-center py-3 px-2 text-xs font-medium rounded-lg text-blue-600 hover:bg-blue-50/50 transition-all"
          >
            <Plus className="h-5 w-5 mb-1" />
            <span>Create</span>
          </button>
        </div>
      </div>

      {/* Spacer for mobile navigation */}
      <div className="md:hidden h-24" />
    </div>
  );
};

export default Layout;