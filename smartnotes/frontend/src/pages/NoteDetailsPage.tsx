import React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Edit, Trash2, Calendar, Tag, Clock, FileText, BookOpen, Sparkles } from 'lucide-react';
import { useNote, useDeleteNote } from '../hooks/useNotes';
import { formatDate } from '../utils';

const NoteDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: note, isLoading, error } = useNote(id!);
  const deleteNoteMutation = useDeleteNote();

  const handleDelete = async () => {
    if (!note || !window.confirm('Are you sure you want to delete this note? This action cannot be undone.')) {
      return;
    }

    try {
      await deleteNoteMutation.mutateAsync(note.id);
      navigate('/');
    } catch (error) {
      console.error('Failed to delete note:', error);
    }
  };

  const getGradientForNote = (noteId: string) => {
    const gradients = [
      'from-blue-500 to-purple-600',
      'from-green-500 to-teal-600',
      'from-orange-500 to-red-600',
      'from-pink-500 to-rose-600',
      'from-indigo-500 to-blue-600',
      'from-purple-500 to-pink-600'
    ];
    return gradients[Math.abs(noteId.split('').reduce((a, b) => a + b.charCodeAt(0), 0)) % gradients.length];
  };

  if (isLoading) {
    return (
      <div className="max-w-5xl mx-auto">
        <div className="space-y-8">
          {/* Header Skeleton */}
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="loading-skeleton w-12 h-12 rounded-xl"></div>
              <div className="loading-skeleton w-48 h-8 rounded-lg"></div>
            </div>
            <div className="flex space-x-3">
              <div className="loading-skeleton w-24 h-12 rounded-xl"></div>
              <div className="loading-skeleton w-24 h-12 rounded-xl"></div>
            </div>
          </div>
          
          {/* Content Skeleton */}
          <div className="modern-card">
            <div className="p-8 space-y-6">
              <div className="loading-skeleton h-10 w-3/4 rounded-lg"></div>
              <div className="loading-skeleton h-6 w-1/2 rounded-lg"></div>
              <div className="space-y-3">
                <div className="loading-skeleton h-4 w-full rounded"></div>
                <div className="loading-skeleton h-4 w-full rounded"></div>
                <div className="loading-skeleton h-4 w-3/4 rounded"></div>
              </div>
              <div className="flex space-x-2">
                <div className="loading-skeleton h-8 w-20 rounded-full"></div>
                <div className="loading-skeleton h-8 w-16 rounded-full"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !note) {
    return (
      <div className="max-w-5xl mx-auto">
        <div className="empty-state">
          <div className="modern-card p-12">
            <div className="empty-state-icon bg-gradient-to-br from-red-100 to-orange-100 rounded-2xl flex items-center justify-center">
              <FileText className="w-8 h-8 text-red-600" />
            </div>
            <h3 className="empty-state-title">
              Note not found
            </h3>
            <p className="empty-state-description">
              The note you're looking for doesn't exist or has been deleted.
            </p>
            <Link to="/" className="btn-modern">
              <ArrowLeft className="w-4 h-4" />
              Back to Notes
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-8">
      {/* Modern Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate(-1)}
            className="btn-icon-modern"
            title="Go back"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center">
              <BookOpen className="w-7 h-7 mr-3 text-blue-600" />
              Note Details
            </h1>
            <p className="text-gray-600 mt-1">
              View and manage your note
            </p>
          </div>
        </div>
        
        <div className="flex items-center space-x-3">
          <Link
            to={`/notes/${note.id}/edit`}
            className="btn-secondary-modern"
          >
            <Edit className="w-4 h-4" />
            Edit Note
          </Link>
          <button
            onClick={handleDelete}
            disabled={deleteNoteMutation.isPending}
            className="btn-danger-modern"
          >
            <Trash2 className="w-4 h-4" />
            {deleteNoteMutation.isPending ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>

      {/* Note Content Card */}
      <div className="modern-card overflow-hidden">
        {/* Header with gradient */}
        <div className={`bg-gradient-to-r ${getGradientForNote(note.id)} p-8 text-white`}>
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <h2 className="text-4xl font-bold mb-2 break-words">
                {note.title}
              </h2>
              <div className="flex flex-wrap items-center gap-4 text-white/80">
                <div className="flex items-center">
                  <Calendar className="w-4 h-4 mr-2" />
                  Created {formatDate(note.createdAt)}
                </div>
                {note.updatedAt !== note.createdAt && (
                  <div className="flex items-center">
                    <Clock className="w-4 h-4 mr-2" />
                    Updated {formatDate(note.updatedAt)}
                  </div>
                )}
              </div>
            </div>
            <div className="ml-6">
              <div className="w-16 h-16 bg-white/20 rounded-2xl flex items-center justify-center backdrop-blur-sm">
                <FileText className="w-8 h-8 text-white" />
              </div>
            </div>
          </div>
        </div>

        {/* Content Section */}
        <div className="p-8 space-y-8">
          {/* Tags Section */}
          {note.tags.length > 0 && (
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900 flex items-center">
                <Tag className="w-5 h-5 mr-2 text-blue-600" />
                Tags
                <span className="ml-2 px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs">
                  {note.tags.length}
                </span>
              </h3>
              <div className="flex flex-wrap gap-3">
                {note.tags.map((tag, index) => (
                  <span
                    key={index}
                    className="modern-tag bg-gradient-to-r from-blue-100 to-purple-100 text-blue-800 border border-blue-200"
                  >
                    <Tag className="w-3 h-3 mr-1" />
                    {tag}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Content Section */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-gray-900 flex items-center">
              <Sparkles className="w-5 h-5 mr-2 text-blue-600" />
              Content
            </h3>
            <div className="content-display">
              {note.content ? (
                <div className="prose prose-lg max-w-none">
                  <div className="whitespace-pre-wrap text-gray-800 leading-relaxed">
                    {note.content}
                  </div>
                </div>
              ) : (
                <div className="empty-content">
                  <div className="flex items-center justify-center py-12 text-gray-500">
                    <div className="text-center">
                      <FileText className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                      <p className="text-lg font-medium mb-2">No content available</p>
                      <p className="text-sm">This note doesn't have any content yet.</p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Stats Section */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-6 border-t border-gray-200">
            <div className="stat-card">
              <div className="flex items-center">
                <div className="stat-icon bg-blue-100 text-blue-600">
                  <FileText className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Characters</p>
                  <p className="stat-value">{note.content?.length || 0}</p>
                </div>
              </div>
            </div>
            <div className="stat-card">
              <div className="flex items-center">
                <div className="stat-icon bg-green-100 text-green-600">
                  <Tag className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Tags</p>
                  <p className="stat-value">{note.tags.length}</p>
                </div>
              </div>
            </div>
            <div className="stat-card">
              <div className="flex items-center">
                <div className="stat-icon bg-purple-100 text-purple-600">
                  <Clock className="w-4 h-4" />
                </div>
                <div>
                  <p className="stat-label">Last Updated</p>
                  <p className="stat-value text-sm">{formatDate(note.updatedAt)}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Error Display */}
      {deleteNoteMutation.error && (
        <div className="modern-card p-6 border-2 border-red-200 bg-red-50">
          <div className="flex items-center">
            <Trash2 className="w-5 h-5 text-red-600 mr-3" />
            <p className="text-red-700 font-medium">
              {deleteNoteMutation.error.message || 'Failed to delete note. Please try again.'}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default NoteDetailsPage;