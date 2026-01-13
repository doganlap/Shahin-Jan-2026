'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';

interface Evidence {
  id: string;
  name: string;
  type: string;
  status: string;
  controlId: string;
  uploadedBy: string;
  uploadedAt: string;
  expiresAt: string | null;
}

export default function EvidencePage() {
  const [evidence, setEvidence] = useState<Evidence[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadEvidence();
  }, []);

  async function loadEvidence() {
    try {
      const response = await api.getEvidence({ page: 1, size: 100 });
      if (response.success && response.data?.items) {
        setEvidence(response.data.items);
      }
    } catch (error) {
      console.error('Error loading evidence:', error);
    } finally {
      setLoading(false);
    }
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Approved: 'bg-green-100 text-green-700',
      Pending: 'bg-yellow-100 text-yellow-700',
      Rejected: 'bg-red-100 text-red-700',
      Expired: 'bg-gray-100 text-gray-700',
    };
    return styles[status] || 'bg-gray-100 text-gray-700';
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Evidence Manager</h1>
          <p className="text-gray-600">Shahin PROVE/VAULT - Upload and manage compliance evidence</p>
        </div>
        <div className="flex space-x-3">
          <button className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
            Bulk Upload
          </button>
          <button className="px-4 py-2 bg-[#0E7490] text-white rounded-lg hover:bg-[#0A5D73]">
            + Upload Evidence
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4 mb-6">
        <div className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm">
          <div className="text-2xl font-bold text-gray-900">{evidence.length}</div>
          <div className="text-sm text-gray-500">Total Evidence</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm">
          <div className="text-2xl font-bold text-green-600">
            {evidence.filter(e => e.status === 'Approved').length}
          </div>
          <div className="text-sm text-gray-500">Approved</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm">
          <div className="text-2xl font-bold text-yellow-600">
            {evidence.filter(e => e.status === 'Pending').length}
          </div>
          <div className="text-sm text-gray-500">Pending Review</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm">
          <div className="text-2xl font-bold text-red-600">
            {evidence.filter(e => e.status === 'Expired').length}
          </div>
          <div className="text-sm text-gray-500">Expiring Soon</div>
        </div>
      </div>

      {/* Evidence Grid */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search evidence..."
            className="w-full max-w-md px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#0E7490]"
          />
        </div>
        <table className="w-full">
          <thead className="bg-gray-50 border-b border-gray-100">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Type</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Control</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Uploaded</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Expires</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                  Loading evidence...
                </td>
              </tr>
            ) : evidence.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                  No evidence uploaded yet. Click "Upload Evidence" to add your first document.
                </td>
              </tr>
            ) : (
              evidence.map((item) => (
                <tr key={item.id} className="border-b border-gray-50 hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="flex items-center space-x-2">
                      <span className="text-xl">📄</span>
                      <span className="font-medium text-gray-900">{item.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{item.type}</td>
                  <td className="px-4 py-3">
                    <span className="px-2 py-1 bg-blue-100 text-blue-700 text-xs rounded">
                      {item.controlId}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 text-xs rounded ${getStatusBadge(item.status)}`}>
                      {item.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600 text-sm">{item.uploadedAt}</td>
                  <td className="px-4 py-3 text-gray-600 text-sm">{item.expiresAt || '-'}</td>
                  <td className="px-4 py-3">
                    <div className="flex space-x-2">
                      <button className="p-1 text-blue-600 hover:bg-blue-50 rounded" title="Download">
                        ⬇️
                      </button>
                      <button className="p-1 text-gray-600 hover:bg-gray-100 rounded" title="View">
                        👁️
                      </button>
                      <button className="p-1 text-red-600 hover:bg-red-50 rounded" title="Delete">
                        🗑️
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
