import React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Edit, Trash2, Calendar, Tag } from 'lucide-react';
import { useNote, useDeleteNote } from '../hooks/useNotes';
import { formatDate } from '../utils';

const NoteDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: note, isLoading, error } = useNote(id!);
  const deleteNoteMutation = useDeleteNote();

  const handleDelete = async () => {
    if (!note || !window.confirm('Are you sure you want to delete this note?')) {
      return;
    }

    try {
      await deleteNoteMutation.mutateAsync(note.id);
      navigate('/');
    } catch (error) {
      console.error('Failed to delete note:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto">
        <div className="animate-pulse">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center space-x-4">
              <div className="h-10 w-10 bg-gray-200 rounded"></div>
              <div className="h-8 w-64 bg-gray-200 rounded"></div>
            </div>
            <div className="flex space-x-3">
              <div className="h-10 w-20 bg-gray-200 rounded"></div>
              <div className="h-10 w-20 bg-gray-200 rounded"></div>
            </div>
          </div>
          
          <div className="card p-6 space-y-4">
            <div className="h-6 bg-gray-200 rounded"></div>
            <div className="h-4 bg-gray-200 rounded w-1/3"></div>
            <div className="space-y-2">
              <div className="h-4 bg-gray-200 rounded"></div>
              <div className="h-4 bg-gray-200 rounded"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4"></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !note) {
    return (
      <div className="max-w-4xl mx-auto text-center py-12">
        <h2 className="text-xl font-semibold text-gray-900 mb-2">
          Note not found
        </h2>
        <p className="text-gray-600 mb-6">
          The note you're looking for doesn't exist or has been deleted.
        </p>
        <Link to="/" className="btn-primary">
          Back to Notes
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate(-1)}
            className="btn-ghost h-10 w-10 p-0"
          >
            <ArrowLeft className="h-4 w-4" />
          </button>
          <h1 className="text-2xl font-bold text-gray-900">Note Details</h1>
        </div>
        
        <div className="flex items-center space-x-3">
          <Link
            to={`/notes/${note.id}/edit`}
            className="btn-secondary h-10 px-4"
          >
            <Edit className="h-4 w-4 mr-2" />
            Edit
          </Link>
          <button
            onClick={handleDelete}
            disabled={deleteNoteMutation.isPending}
            className="btn-ghost h-10 px-4 text-red-600 hover:bg-red-50 hover:text-red-700"
          >
            <Trash2 className="h-4 w-4 mr-2" />
            {deleteNoteMutation.isPending ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>

      {/* Note Content */}
      <div className="card p-6 space-y-6">
        {/* Title */}
        <div>
          <h2 className="text-3xl font-bold text-gray-900">{note.title}</h2>
        </div>

        {/* Metadata */}
        <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600 border-b pb-4">
          <div className="flex items-center">
            <Calendar className="h-4 w-4 mr-1" />
            Created: {formatDate(note.createdAt)}
          </div>
          {note.updatedAt !== note.createdAt && (
            <div className="flex items-center">
              <Calendar className="h-4 w-4 mr-1" />
              Updated: {formatDate(note.updatedAt)}
            </div>
          )}
        </div>

        {/* Tags */}
        {note.tags.length > 0 && (
          <div>
            <h3 className="text-sm font-medium text-gray-700 mb-2 flex items-center">
              <Tag className="h-4 w-4 mr-1" />
              Tags
            </h3>
            <div className="flex flex-wrap gap-2">
              {note.tags.map((tag, index) => (
                <span key={index} className="tag">
                  {tag}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Content */}
        <div>
          <h3 className="text-sm font-medium text-gray-700 mb-3">Content</h3>
          <div className="prose max-w-none">
            {note.content ? (
              <div className="whitespace-pre-wrap text-gray-900">
                {note.content}
              </div>
            ) : (
              <p className="text-gray-500 italic">No content</p>
            )}
          </div>
        </div>
      </div>

      {/* Error Display */}
      {deleteNoteMutation.error && (
        <div className="card p-4 border-red-200 bg-red-50 mt-4">
          <p className="text-sm text-red-600">
            {deleteNoteMutation.error.message || 'Failed to delete note. Please try again.'}
          </p>
        </div>
      )}
    </div>
  );
};

export default NoteDetailsPage;