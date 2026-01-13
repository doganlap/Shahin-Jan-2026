'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';

interface Control {
  id: string;
  code: string;
  name: string;
  framework: string;
  category: string;
  status: string;
  maturity: string | null;
  lastAssessed: string | null;
}

export default function ControlsPage() {
  const [controls, setControls] = useState<Control[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState({ framework: '', status: '' });

  useEffect(() => {
    loadControls();
  }, []);

  async function loadControls() {
    try {
      const response = await api.getControls({ page: 1, size: 100 });
      if (response.success && response.data?.items) {
        setControls(response.data.items.map((c: any) => ({
          id: c.id,
          code: c.code || c.controlCode || c.name?.substring(0, 10),
          name: c.name || c.controlName,
          framework: c.frameworkCode || c.framework || 'N/A',
          category: c.category || 'N/A',
          status: c.complianceStatus || c.status || 'NotAssessed',
          maturity: c.maturityLevel || c.maturity || null,
          lastAssessed: c.lastAssessedDate || c.lastAssessed || null
        })));
      }
    } catch (error) {
      console.error('Error loading controls:', error);
    } finally {
      setLoading(false);
    }
  }

  const filteredControls = controls.filter(c => {
    if (search && !c.code.toLowerCase().includes(search.toLowerCase()) &&
        !c.name.toLowerCase().includes(search.toLowerCase())) return false;
    if (filter.framework && c.framework !== filter.framework) return false;
    if (filter.status && c.status !== filter.status) return false;
    return true;
  });

  const stats = {
    total: controls.length,
    compliant: controls.filter(c => c.status === 'Compliant').length,
    partial: controls.filter(c => c.status === 'Partial').length,
    nonCompliant: controls.filter(c => c.status === 'NonCompliant').length,
    notAssessed: controls.filter(c => c.status === 'NotAssessed').length,
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Compliant: 'bg-green-100 text-green-700',
      Partial: 'bg-yellow-100 text-yellow-700',
      NonCompliant: 'bg-red-100 text-red-700',
      NotAssessed: 'bg-gray-100 text-gray-700',
    };
    return styles[status] || 'bg-gray-100 text-gray-700';
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Controls Library</h1>
          <p className="text-gray-600">Shahin MAP - Manage your control library</p>
        </div>
        <div className="flex space-x-3">
          <button className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
            Export
          </button>
          <button className="px-4 py-2 bg-[#0E7490] text-white rounded-lg hover:bg-[#0A5D73]">
            + Add Control
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-5 gap-4 mb-6">
        <div className="bg-blue-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.total}</div>
          <div className="text-sm opacity-90">Total</div>
        </div>
        <div className="bg-green-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.compliant}</div>
          <div className="text-sm opacity-90">Compliant</div>
        </div>
        <div className="bg-yellow-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.partial}</div>
          <div className="text-sm opacity-90">Partial</div>
        </div>
        <div className="bg-red-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.nonCompliant}</div>
          <div className="text-sm opacity-90">Non-Compliant</div>
        </div>
        <div className="bg-gray-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.notAssessed}</div>
          <div className="text-sm opacity-90">Not Assessed</div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
        <div className="grid grid-cols-4 gap-4">
          <input
            type="text"
            placeholder="Search controls..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#0E7490] focus:border-transparent"
          />
          <select
            value={filter.framework}
            onChange={(e) => setFilter({ ...filter, framework: e.target.value })}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#0E7490]"
          >
            <option value="">All Frameworks</option>
            <option value="NCA-ECC">NCA ECC</option>
            <option value="SAMA-CSF">SAMA CSF</option>
            <option value="PDPL">PDPL</option>
            <option value="NCA-CSCC">NCA CSCC</option>
          </select>
          <select
            value={filter.status}
            onChange={(e) => setFilter({ ...filter, status: e.target.value })}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#0E7490]"
          >
            <option value="">All Status</option>
            <option value="Compliant">Compliant</option>
            <option value="Partial">Partial</option>
            <option value="NonCompliant">Non-Compliant</option>
            <option value="NotAssessed">Not Assessed</option>
          </select>
          <div className="text-right text-gray-500 self-center">
            {filteredControls.length} controls
          </div>
        </div>
      </div>

      {/* Controls Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 border-b border-gray-100">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Control ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Framework</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Category</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Maturity</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                  Loading controls...
                </td>
              </tr>
            ) : filteredControls.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                  No controls found
                </td>
              </tr>
            ) : (
              filteredControls.map((control) => (
                <tr key={control.id} className="border-b border-gray-50 hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <a href={`/controls/${control.id}`} className="font-medium text-[#0E7490] hover:underline">
                      {control.code}
                    </a>
                  </td>
                  <td className="px-4 py-3 text-gray-900">{control.name}</td>
                  <td className="px-4 py-3">
                    <span className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                      {control.framework}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{control.category}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 text-xs rounded ${getStatusBadge(control.status)}`}>
                      {control.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{control.maturity || '-'}</td>
                  <td className="px-4 py-3">
                    <div className="flex space-x-2">
                      <button className="p-1 text-blue-600 hover:bg-blue-50 rounded" title="Assess">
                        ✅
                      </button>
                      <button className="p-1 text-gray-600 hover:bg-gray-100 rounded" title="Edit">
                        ✏️
                      </button>
                      <button className="p-1 text-gray-600 hover:bg-gray-100 rounded" title="View">
                        👁️
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
