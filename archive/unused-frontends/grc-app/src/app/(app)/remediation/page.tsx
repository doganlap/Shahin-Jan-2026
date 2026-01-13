'use client';

import { useState } from 'react';

interface Issue {
  id: string;
  title: string;
  control: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  status: 'Open' | 'InProgress' | 'Review' | 'Closed';
  assignee: string;
  dueDate: string;
}

export default function RemediationPage() {
  const [issues] = useState<Issue[]>([
    { id: 'ISS-001', title: 'Missing MFA for admin accounts', control: 'ECC-2.2.2', severity: 'Critical', status: 'Open', assignee: 'John D.', dueDate: '2026-01-10' },
    { id: 'ISS-002', title: 'Incomplete audit logs', control: 'ECC-3.1.1', severity: 'High', status: 'InProgress', assignee: 'Sarah M.', dueDate: '2026-01-15' },
    { id: 'ISS-003', title: 'Security training incomplete', control: 'SAMA-2.1', severity: 'Medium', status: 'Review', assignee: 'Mike R.', dueDate: '2026-01-20' },
    { id: 'ISS-004', title: 'Encryption not enabled', control: 'PDPL-8', severity: 'Critical', status: 'InProgress', assignee: 'John D.', dueDate: '2026-01-12' },
  ]);

  const getSeverityBadge = (severity: string) => {
    const styles: Record<string, string> = {
      Critical: 'bg-red-100 text-red-700',
      High: 'bg-orange-100 text-orange-700',
      Medium: 'bg-yellow-100 text-yellow-700',
      Low: 'bg-blue-100 text-blue-700',
    };
    return styles[severity] || 'bg-gray-100 text-gray-700';
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Open: 'bg-gray-100 text-gray-700',
      InProgress: 'bg-blue-100 text-blue-700',
      Review: 'bg-yellow-100 text-yellow-700',
      Closed: 'bg-green-100 text-green-700',
    };
    return styles[status] || 'bg-gray-100 text-gray-700';
  };

  const stats = {
    critical: issues.filter(i => i.severity === 'Critical').length,
    high: issues.filter(i => i.severity === 'High').length,
    medium: issues.filter(i => i.severity === 'Medium').length,
    open: issues.filter(i => i.status === 'Open').length,
    inProgress: issues.filter(i => i.status === 'InProgress').length,
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Remediation Manager</h1>
          <p className="text-gray-600">Shahin FIX - Track and resolve compliance gaps</p>
        </div>
        <button className="px-4 py-2 bg-[#0E7490] text-white rounded-lg hover:bg-[#0A5D73]">
          + New Issue
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-5 gap-4 mb-6">
        <div className="bg-red-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.critical}</div>
          <div className="text-sm opacity-90">Critical</div>
        </div>
        <div className="bg-orange-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.high}</div>
          <div className="text-sm opacity-90">High</div>
        </div>
        <div className="bg-yellow-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.medium}</div>
          <div className="text-sm opacity-90">Medium</div>
        </div>
        <div className="bg-gray-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.open}</div>
          <div className="text-sm opacity-90">Open</div>
        </div>
        <div className="bg-blue-500 text-white p-4 rounded-lg text-center">
          <div className="text-2xl font-bold">{stats.inProgress}</div>
          <div className="text-sm opacity-90">In Progress</div>
        </div>
      </div>

      {/* Kanban Board */}
      <div className="grid grid-cols-4 gap-4 mb-6">
        {['Open', 'InProgress', 'Review', 'Closed'].map((status) => (
          <div key={status} className="bg-white rounded-xl shadow-sm border border-gray-100">
            <div className={`p-3 border-b font-medium ${
              status === 'Open' ? 'bg-gray-100' :
              status === 'InProgress' ? 'bg-blue-100' :
              status === 'Review' ? 'bg-yellow-100' : 'bg-green-100'
            }`}>
              {status === 'InProgress' ? 'In Progress' : status}
              <span className="ml-2 px-2 py-0.5 bg-white rounded text-sm">
                {issues.filter(i => i.status === status).length}
              </span>
            </div>
            <div className="p-2 space-y-2 min-h-[200px]">
              {issues.filter(i => i.status === status).map((issue) => (
                <div key={issue.id} className={`p-3 rounded-lg border-l-4 bg-gray-50 ${
                  issue.severity === 'Critical' ? 'border-red-500' :
                  issue.severity === 'High' ? 'border-orange-500' :
                  issue.severity === 'Medium' ? 'border-yellow-500' : 'border-blue-500'
                }`}>
                  <div className="flex justify-between items-start mb-1">
                    <span className="font-medium text-sm">{issue.id}</span>
                    <span className={`text-xs px-1.5 py-0.5 rounded ${getSeverityBadge(issue.severity)}`}>
                      {issue.severity}
                    </span>
                  </div>
                  <p className="text-sm text-gray-700 mb-2">{issue.title}</p>
                  <div className="flex justify-between text-xs text-gray-500">
                    <span>{issue.assignee}</span>
                    <span>{issue.dueDate}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Issues Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900">All Issues</h2>
        </div>
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Issue ID</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Title</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Control</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Severity</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Assignee</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Due Date</th>
            </tr>
          </thead>
          <tbody>
            {issues.map((issue) => (
              <tr key={issue.id} className="border-b border-gray-50 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-[#0E7490]">{issue.id}</td>
                <td className="px-4 py-3 text-gray-900">{issue.title}</td>
                <td className="px-4 py-3">
                  <span className="px-2 py-1 bg-blue-100 text-blue-700 text-xs rounded">{issue.control}</span>
                </td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 text-xs rounded ${getSeverityBadge(issue.severity)}`}>{issue.severity}</span>
                </td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 text-xs rounded ${getStatusBadge(issue.status)}`}>{issue.status}</span>
                </td>
                <td className="px-4 py-3 text-gray-600">{issue.assignee}</td>
                <td className="px-4 py-3 text-gray-600">{issue.dueDate}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
