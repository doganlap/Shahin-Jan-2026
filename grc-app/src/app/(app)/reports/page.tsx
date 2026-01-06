'use client';

import { useState } from 'react';

export default function ReportsPage() {
  const [selectedReport, setSelectedReport] = useState('');

  const reportTypes = [
    { id: 'compliance', name: 'Compliance Summary', icon: '📊', description: 'Overall compliance status by framework' },
    { id: 'controls', name: 'Controls Report', icon: '🛡️', description: 'Detailed control assessment results' },
    { id: 'gaps', name: 'Gap Analysis', icon: '🔍', description: 'Identified gaps and remediation status' },
    { id: 'evidence', name: 'Evidence Report', icon: '📁', description: 'Evidence collection and validity status' },
    { id: 'risk', name: 'Risk Report', icon: '⚠️', description: 'Risk register and treatment plans' },
    { id: 'audit', name: 'Audit Package', icon: '📋', description: 'Complete audit-ready documentation' },
  ];

  const recentReports = [
    { name: 'Q4 2025 Compliance Summary', type: 'compliance', date: '2025-12-31', status: 'Complete' },
    { name: 'NCA ECC Gap Analysis', type: 'gaps', date: '2025-12-15', status: 'Complete' },
    { name: 'SAMA CSF Controls Report', type: 'controls', date: '2025-12-10', status: 'Complete' },
  ];

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Reports & Analytics</h1>
          <p className="text-gray-600">Shahin WATCH - Generate compliance reports and dashboards</p>
        </div>
        <button className="px-4 py-2 bg-[#0E7490] text-white rounded-lg hover:bg-[#0A5D73]">
          + Generate Report
        </button>
      </div>

      {/* Report Types */}
      <div className="grid grid-cols-3 gap-4 mb-8">
        {reportTypes.map((report) => (
          <button
            key={report.id}
            onClick={() => setSelectedReport(report.id)}
            className={`p-4 rounded-xl border text-left transition-all ${
              selectedReport === report.id
                ? 'border-[#0E7490] bg-[#0E7490]/5'
                : 'border-gray-200 hover:border-[#0E7490]/50'
            }`}
          >
            <div className="text-3xl mb-2">{report.icon}</div>
            <h3 className="font-semibold text-gray-900">{report.name}</h3>
            <p className="text-sm text-gray-500">{report.description}</p>
          </button>
        ))}
      </div>

      {/* Recent Reports */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-4 border-b border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900">Recent Reports</h2>
        </div>
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Report Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Type</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Generated</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody>
            {recentReports.map((report, index) => (
              <tr key={index} className="border-b border-gray-50 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-gray-900">{report.name}</td>
                <td className="px-4 py-3 text-gray-600 capitalize">{report.type}</td>
                <td className="px-4 py-3 text-gray-600">{report.date}</td>
                <td className="px-4 py-3">
                  <span className="px-2 py-1 bg-green-100 text-green-700 text-xs rounded">
                    {report.status}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <div className="flex space-x-2">
                    <button className="p-1 text-blue-600 hover:bg-blue-50 rounded">📥</button>
                    <button className="p-1 text-gray-600 hover:bg-gray-100 rounded">👁️</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
