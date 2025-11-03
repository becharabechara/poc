import React, { useState } from 'react';
import { Search, Tag, X, FileText, Filter, Sparkles, Hash, Clock } from 'lucide-react';
import { useSearchNotes, useTags } from '../hooks/useNotes';
import { SearchNotesRequest } from '../types';
import { Link } from 'react-router-dom';
import { formatDate } from '../utils';

const SearchPage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState<SearchNotesRequest>({
    keyword: '',
    tags: [],
  });
  const [tagInput, setTagInput] = useState('');
  
  const { data: availableTags } = useTags();
  const { data: searchResults, isLoading, error } = useSearchNotes(searchQuery);

  const hasActiveSearch = searchQuery.keyword || (searchQuery.tags && searchQuery.tags.length > 0);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    // Search is automatically triggered by useSearchNotes when searchQuery changes
  };

  const addTag = (tag: string) => {
    if (!searchQuery.tags?.includes(tag)) {
      setSearchQuery({
        ...searchQuery,
        tags: [...(searchQuery.tags || []), tag],
      });
    }
  };

  const removeTag = (tagToRemove: string) => {
    setSearchQuery({
      ...searchQuery,
      tags: searchQuery.tags?.filter(tag => tag !== tagToRemove) || [],
    });
  };

  const addCustomTag = () => {
    const tag = tagInput.trim().toLowerCase();
    if (tag) {
      addTag(tag);
      setTagInput('');
    }
  };

  const handleTagInputKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addCustomTag();
    }
  };

  const clearSearch = () => {
    setSearchQuery({ keyword: '', tags: [] });
    setTagInput('');
  };

  const getRandomGradient = (id: string) => {
    const gradients = [
      'from-blue-500 to-purple-600',
      'from-green-500 to-teal-600',
      'from-orange-500 to-red-600',
      'from-pink-500 to-rose-600',
      'from-indigo-500 to-blue-600',
      'from-purple-500 to-pink-600'
    ];
    return gradients[Math.abs(id.split('').reduce((a, b) => a + b.charCodeAt(0), 0)) % gradients.length];
  };

  return (
    <div className="max-w-6xl mx-auto space-y-8">
      {/* Modern Header */}
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">
          Discover Your Notes ✨
        </h1>
        <p className="text-lg text-gray-600">
          Search through your knowledge with powerful filters
        </p>
      </div>

      {/* Enhanced Search Form */}
      <div className="modern-form space-y-8">
        <form onSubmit={handleSearch} className="space-y-6">
          {/* Keyword Search */}
          <div className="form-group">
            <label htmlFor="keyword" className="form-label flex items-center">
              <Search className="w-4 h-4 mr-2 text-blue-600" />
              Search Everything
            </label>
            <div className="search-container">
              <Search className="search-icon" />
              <input
                type="text"
                id="keyword"
                value={searchQuery.keyword || ''}
                onChange={(e) => setSearchQuery({ ...searchQuery, keyword: e.target.value })}
                className="search-input"
                placeholder="Search in titles, content, and more..."
              />
            </div>
          </div>

          {/* Tag Selection */}
          <div className="form-group">
            <label className="form-label flex items-center">
              <Hash className="w-4 h-4 mr-2 text-blue-600" />
              Filter by Tags
              {searchQuery.tags && searchQuery.tags.length > 0 && (
                <span className="ml-2 px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs">
                  {searchQuery.tags.length} selected
                </span>
              )}
            </label>
            
            <div className="space-y-4">
              {/* Selected Tags */}
              {searchQuery.tags && searchQuery.tags.length > 0 && (
                <div>
                  <p className="text-sm text-gray-600 mb-3">Selected filters:</p>
                  <div className="flex flex-wrap gap-3">
                    {searchQuery.tags.map((tag, index) => (
                      <span
                        key={index}
                        className="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm bg-gradient-to-r from-blue-100 to-purple-100 text-blue-800 border border-blue-200"
                      >
                        <Hash className="w-3 h-3" />
                        {tag}
                        <button
                          type="button"
                          onClick={() => removeTag(tag)}
                          className="hover:text-red-600 transition-colors ml-1"
                        >
                          <X className="w-3 h-3" />
                        </button>
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Available Tags */}
              {availableTags && availableTags.length > 0 && (
                <div>
                  <p className="text-sm text-gray-600 mb-3">Popular tags:</p>
                  <div className="flex flex-wrap gap-2">
                    {availableTags
                      .filter(tag => !searchQuery.tags?.includes(tag))
                      .slice(0, 10)
                      .map((tag, index) => (
                        <button
                          key={index}
                          type="button"
                          onClick={() => addTag(tag)}
                          className="modern-tag hover:bg-blue-100 cursor-pointer transition-colors"
                        >
                          <Hash className="w-3 h-3 mr-1" />
                          {tag}
                        </button>
                      ))}
                  </div>
                </div>
              )}

              {/* Custom Tag Input */}
              <div className="flex space-x-3">
                <div className="flex-1 relative">
                  <input
                    type="text"
                    value={tagInput}
                    onChange={(e) => setTagInput(e.target.value)}
                    onKeyPress={handleTagInputKeyPress}
                    className="modern-input pl-10"
                    placeholder="Add custom tag..."
                    maxLength={50}
                  />
                  <Hash className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                </div>
                <button
                  type="button"
                  onClick={addCustomTag}
                  disabled={!tagInput.trim()}
                  className="btn-modern"
                >
                  <Tag className="w-4 h-4" />
                  Add
                </button>
              </div>
            </div>
          </div>

          {/* Search Actions */}
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 pt-6 border-t border-gray-200">
            <button
              type="button"
              onClick={clearSearch}
              disabled={!hasActiveSearch}
              className="btn-secondary-modern"
            >
              <X className="w-4 h-4" />
              Clear All Filters
            </button>
            
            <div className="flex items-center text-sm text-gray-600 bg-gray-50 px-4 py-2 rounded-lg">
              <Filter className="w-4 h-4 mr-2" />
              {hasActiveSearch ? (
                isLoading ? (
                  'Searching...'
                ) : (
                  <span>
                    <span className="font-semibold text-gray-900">{searchResults?.length || 0}</span> results found
                  </span>
                )
              ) : (
                'Enter search criteria above'
              )}
            </div>
          </div>
        </form>
      </div>

      {/* Search Results */}
      {hasActiveSearch && (
        <div>
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-gray-900">
              Search Results
            </h2>
            {searchResults && searchResults.length > 0 && (
              <div className="text-sm text-gray-500">
                Found {searchResults.length} {searchResults.length === 1 ? 'note' : 'notes'}
              </div>
            )}
          </div>
          
          {isLoading ? (
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
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : error ? (
            <div className="modern-card p-8 border-2 border-red-200 bg-red-50">
              <div className="flex items-center">
                <X className="w-5 h-5 text-red-600 mr-3" />
                <p className="text-red-700 font-medium">
                  {error.message || 'Failed to search notes. Please try again.'}
                </p>
              </div>
            </div>
          ) : !searchResults || searchResults.length === 0 ? (
            <div className="empty-state">
              <div className="modern-card p-12">
                <div className="empty-state-icon bg-gradient-to-br from-gray-100 to-gray-200 rounded-2xl flex items-center justify-center">
                  <Search className="w-8 h-8 text-gray-500" />
                </div>
                <h3 className="empty-state-title">
                  No notes match your search
                </h3>
                <p className="empty-state-description">
                  Try adjusting your search criteria or explore different tags.
                </p>
                <button
                  onClick={clearSearch}
                  className="btn-modern"
                >
                  <Sparkles className="w-4 h-4" />
                  Clear Search
                </button>
              </div>
            </div>
          ) : (
            <div className="notes-grid">
              {searchResults.map((note) => (
                <Link
                  key={note.id}
                  to={`/notes/${note.id}`}
                  className="modern-card note-card"
                >
                  <div className="note-card-content">
                    <div className="flex justify-between items-start mb-4">
                      <h3 className="note-title">
                        {note.title}
                      </h3>
                      <div className={`w-3 h-3 rounded-full bg-gradient-to-r ${getRandomGradient(note.id)} opacity-60`}></div>
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
              ))}
            </div>
          )}
        </div>
      )}

      {/* Initial State */}
      {!hasActiveSearch && (
        <div className="empty-state">
          <div className="modern-card p-12">
            <div className="empty-state-icon bg-gradient-to-br from-blue-100 to-purple-100 rounded-2xl flex items-center justify-center">
              <Search className="w-8 h-8 text-blue-600" />
            </div>
            <h3 className="empty-state-title">
              Ready to find your notes?
            </h3>
            <p className="empty-state-description">
              Use the search bar above to find notes by keywords, or filter by tags to discover related content.
            </p>
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <button
                onClick={() => setSearchQuery({ ...searchQuery, keyword: 'important' })}
                className="btn-secondary-modern"
              >
                <Sparkles className="w-4 h-4" />
                Search "important"
              </button>
              <Link to="/" className="btn-modern">
                <FileText className="w-4 h-4" />
                Browse All Notes
              </Link>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchPage;