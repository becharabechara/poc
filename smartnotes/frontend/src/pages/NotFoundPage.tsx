import React from 'react';
import { Link } from 'react-router-dom';
import { Home, Search, FileText, ArrowLeft, Compass, Sparkles } from 'lucide-react';

const NotFoundPage: React.FC = () => {
  return (
    <div className="min-h-[70vh] flex items-center justify-center">
      <div className="empty-state">
        <div className="modern-card p-12 max-w-2xl">
          {/* Animated 404 Icon */}
          <div className="relative mb-8">
            <div className="empty-state-icon bg-gradient-to-br from-purple-100 to-pink-100 rounded-3xl flex items-center justify-center mb-6">
              <div className="relative">
                <Compass className="w-12 h-12 text-purple-600" />
                <div className="absolute -top-1 -right-1 w-4 h-4 bg-pink-500 rounded-full animate-ping"></div>
              </div>
            </div>
            
            {/* 404 Text with Gradient */}
            <div className="text-center">
              <h1 className="text-8xl font-black bg-gradient-to-r from-purple-600 via-pink-600 to-blue-600 bg-clip-text text-transparent mb-4">
                404
              </h1>
              <div className="absolute inset-0 flex items-center justify-center opacity-10">
                <div className="text-[12rem] font-black text-gray-200 select-none">
                  404
                </div>
              </div>
            </div>
          </div>

          {/* Content */}
          <div className="relative z-10 text-center">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              Oops! Page Not Found
            </h2>
            <p className="text-lg text-gray-600 mb-8 max-w-md mx-auto leading-relaxed">
              The page you're looking for seems to have wandered off into the digital wilderness. 
              Don't worry, we'll help you find your way back!
            </p>

            {/* Quick Stats */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
              <div className="stat-card">
                <div className="stat-icon bg-blue-100 text-blue-600">
                  <FileText className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Error Code</p>
                  <p className="stat-value">404</p>
                </div>
              </div>
              <div className="stat-card">
                <div className="stat-icon bg-green-100 text-green-600">
                  <Compass className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Status</p>
                  <p className="stat-value text-sm">Lost</p>
                </div>
              </div>
              <div className="stat-card">
                <div className="stat-icon bg-purple-100 text-purple-600">
                  <Sparkles className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Solution</p>
                  <p className="stat-value text-sm">Navigate</p>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/"
                className="btn-modern group"
              >
                <Home className="w-4 h-4 group-hover:scale-110 transition-transform" />
                Back to Home
              </Link>
              <Link
                to="/search"
                className="btn-secondary-modern group"
              >
                <Search className="w-4 h-4 group-hover:scale-110 transition-transform" />
                Search Notes
              </Link>
              <button
                onClick={() => window.history.back()}
                className="btn-outline-modern group"
              >
                <ArrowLeft className="w-4 h-4 group-hover:scale-110 transition-transform" />
                Go Back
              </button>
            </div>

            {/* Helpful Links */}
            <div className="mt-12 pt-8 border-t border-gray-200">
              <p className="text-sm text-gray-500 mb-4">Need help? Try these options:</p>
              <div className="flex flex-wrap justify-center gap-6 text-sm">
                <Link 
                  to="/" 
                  className="text-blue-600 hover:text-blue-700 transition-colors flex items-center"
                >
                  <FileText className="w-3 h-3 mr-1" />
                  Browse All Notes
                </Link>
                <Link 
                  to="/notes/create" 
                  className="text-green-600 hover:text-green-700 transition-colors flex items-center"
                >
                  <Sparkles className="w-3 h-3 mr-1" />
                  Create New Note
                </Link>
                <Link 
                  to="/search" 
                  className="text-purple-600 hover:text-purple-700 transition-colors flex items-center"
                >
                  <Search className="w-3 h-3 mr-1" />
                  Advanced Search
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default NotFoundPage;