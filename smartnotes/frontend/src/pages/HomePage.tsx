import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Search, Plus, FileText, Calendar, Tag, Clock, Sparkles } from 'lucide-react';
import { useNotes } from '../hooks/useNotes';
import { formatDate, cn } from '../utils';
import { NoteResponse, SearchNotesRequest } from '../types';

const HomePage: React.FC = () => {
  const { data: notes, isLoading, error } = useNotes();
  const [searchQuery, setSearchQuery] = useState<SearchNotesRequest>({});

  const filteredNotes = notes?.filter((note) => {
    if (!searchQuery.keyword && (!searchQuery.tags || searchQuery.tags.length === 0)) {
      return true;
    }

    const matchesKeyword = !searchQuery.keyword || 
      note.title.toLowerCase().includes(searchQuery.keyword.toLowerCase()) ||
      note.content.toLowerCase().includes(searchQuery.keyword.toLowerCase());

    const matchesTags = !searchQuery.tags || searchQuery.tags.length === 0 ||
      searchQuery.tags.some(tag => note.tags.includes(tag));

    return matchesKeyword && matchesTags;
  });

  if (isLoading) {
    return (
      <div className="space-y-8">
        <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-4">
          <div>
            <div className="loading-skeleton h-8 w-48 mb-2"></div>
            <div className="loading-skeleton h-4 w-32"></div>
          </div>
          <div className="loading-skeleton h-12 w-32 rounded-lg"></div>
        </div>
        
        <div className="loading-skeleton h-16 rounded-2xl"></div>
        
        <div className="notes-grid">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="modern-card">
              <div className="p-6 space-y-4">
                <div className="loading-skeleton h-6 w-3/4"></div>
                <div className="space-y-2">
                  <div className="loading-skeleton h-4 w-full"></div>
                  <div className="loading-skeleton h-4 w-4/5"></div>
                  <div className="loading-skeleton h-4 w-3/5"></div>
                </div>
                <div className="flex justify-between items-center">
                  <div className="loading-skeleton h-3 w-24"></div>
                  <div className="flex space-x-2">
                    <div className="loading-skeleton h-6 w-16 rounded-full"></div>
                    <div className="loading-skeleton h-6 w-20 rounded-full"></div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="empty-state">
        <div className="modern-card p-8">
          <FileText className="empty-state-icon" />
          <h2 className="empty-state-title">
            Failed to load notes
          </h2>
          <p className="empty-state-description">
            {error.message || 'Something went wrong while loading your notes.'}
          </p>
          <button
            onClick={() => window.location.reload()}
            className="btn-modern"
          >
            <Sparkles className="w-4 h-4" />
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Modern Header */}
      <div className="flex flex-col sm:flex-row sm:justify-between sm:items-end gap-6">
        <div>
          <h1 className="text-4xl font-bold text-gray-900 mb-2">
            Welcome back! ✨
          </h1>
          <p className="text-lg text-gray-600">
            You have <span className="font-semibold text-gray-900">{notes?.length || 0}</span> {notes?.length === 1 ? 'note' : 'notes'} in your collection
          </p>
        </div>
        <Link
          to="/create"
          className="btn-modern"
        >
          <Plus className="w-4 h-4" />
          Create New Note
        </Link>
      </div>

      {/* Enhanced Search Bar */}
      <div className="search-container">
        <Search className="search-icon" />
        <input
          type="text"
          placeholder="Search your notes..."
          className="search-input"
          value={searchQuery.keyword || ''}
          onChange={(e) => setSearchQuery({ ...searchQuery, keyword: e.target.value })}
        />
        <Link
          to="/search"
          className="absolute right-3 top-1/2 transform -translate-y-1/2 btn-secondary-modern py-2 px-4 text-sm"
        >
          Advanced
        </Link>
      </div>

      {/* Notes Grid */}
      {filteredNotes?.length === 0 ? (
        <div className="empty-state">
          {notes?.length === 0 ? (
            <div className="modern-card p-12">
              <div className="empty-state-icon bg-gradient-to-br from-blue-100 to-purple-100 rounded-2xl flex items-center justify-center">
                <FileText className="w-8 h-8 text-blue-600" />
              </div>
              <h2 className="empty-state-title">
                Your note journey starts here
              </h2>
              <p className="empty-state-description">
                Create your first note and start organizing your thoughts, ideas, and important information.
              </p>
              <Link to="/create" className="btn-modern">
                <Plus className="w-4 h-4" />
                Create Your First Note
              </Link>
            </div>
          ) : (
            <div className="modern-card p-12">
              <div className="empty-state-icon bg-gradient-to-br from-orange-100 to-red-100 rounded-2xl flex items-center justify-center">
                <Search className="w-8 h-8 text-orange-600" />
              </div>
              <h2 className="empty-state-title">
                No notes match your search
              </h2>
              <p className="empty-state-description">
                Try adjusting your search terms or explore all your notes.
              </p>
              <div className="flex flex-col sm:flex-row gap-3 justify-center">
                <button
                  onClick={() => setSearchQuery({})}
                  className="btn-secondary-modern"
                >
                  Clear Search
                </button>
                <Link to="/create" className="btn-modern">
                  <Plus className="w-4 h-4" />
                  Create Note
                </Link>
              </div>
            </div>
          )}
        </div>
      ) : (
        <div className="notes-grid">
          {filteredNotes?.map((note) => (
            <NoteCard key={note.id} note={note} />
          ))}
        </div>
      )}
    </div>
  );
};

interface NoteCardProps {
  note: NoteResponse;
}

const NoteCard: React.FC<NoteCardProps> = ({ note }) => {
  const getRandomGradient = () => {
    const gradients = [
      'from-blue-500 to-purple-600',
      'from-green-500 to-teal-600',
      'from-orange-500 to-red-600',
      'from-pink-500 to-rose-600',
      'from-indigo-500 to-blue-600',
      'from-purple-500 to-pink-600'
    ];
    return gradients[Math.abs(note.id.split('').reduce((a, b) => a + b.charCodeAt(0), 0)) % gradients.length];
  };

  return (
    <Link
      to={`/notes/${note.id}`}
      className="modern-card note-card"
    >
      <div className="note-card-content">
        <div className="flex justify-between items-start mb-4">
          <h3 className="note-title">
            {note.title}
          </h3>
          <div className={`w-3 h-3 rounded-full bg-gradient-to-r ${getRandomGradient()} opacity-60`}></div>
        </div>
        
        <p className="note-content-preview">
          {note.content || 'No content available...'}
        </p>
        
        <div className="note-metadata">
          <div className="flex items-center">
            <Clock className="w-3 h-3 mr-1" />
            {formatDate(note.updatedAt)}
          </div>
          <div className="flex items-center">
            <Tag className="w-3 h-3 mr-1" />
            {note.tags.length} {note.tags.length === 1 ? 'tag' : 'tags'}
          </div>
        </div>
        
        {note.tags.length > 0 && (
          <div className="note-tags">
            {note.tags.slice(0, 3).map((tag, index) => (
              <span key={index} className="modern-tag">
                {tag}
              </span>
            ))}
            {note.tags.length > 3 && (
              <span className="modern-tag bg-gray-100 text-gray-600">
                +{note.tags.length - 3} more
              </span>
            )}
          </div>
        )}
      </div>
    </Link>
  );
};

export default HomePage;