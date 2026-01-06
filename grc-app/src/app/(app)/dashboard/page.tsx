'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';

interface DashboardStats {
  activePlans: number;
  completedPlans: number;
  activeBaselines: number;
  estimatedControlCount: number;
  complianceRate: number;
  pendingTasks: number;
}

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  async function loadDashboard() {
    try {
      const response = await api.getDashboard();
      if (response.success && response.data) {
        setStats(response.data.stats || {
          activePlans: 2,
          completedPlans: 5,
          activeBaselines: 6,
          estimatedControlCount: 288,
          complianceRate: 78,
          pendingTasks: 12
        });
      }
    } catch (error) {
      console.error('Error loading dashboard:', error);
    } finally {
      setLoading(false);
    }
  }

  const statCards = [
    { label: 'Active Plans', value: stats?.activePlans || 0, color: 'bg-blue-500', icon: '📋' },
    { label: 'Completed', value: stats?.completedPlans || 0, color: 'bg-green-500', icon: '✅' },
    { label: 'Baselines', value: stats?.activeBaselines || 0, color: 'bg-yellow-500', icon: '📊' },
    { label: 'Controls', value: stats?.estimatedControlCount || 0, color: 'bg-purple-500', icon: '🛡️' },
    { label: 'Compliance Rate', value: `${stats?.complianceRate || 0}%`, color: 'bg-teal-500', icon: '📈' },
    { label: 'Pending Tasks', value: stats?.pendingTasks || 0, color: 'bg-orange-500', icon: '⏳' },
  ];

  return (
    <div>
      {/* Page Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600">Overview of your compliance status</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
        {statCards.map((stat, index) => (
          <div key={index} className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-500">{stat.label}</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : stat.value}</p>
              </div>
              <div className={`${stat.color} text-white p-3 rounded-lg text-xl`}>
                {stat.icon}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Activity */}
        <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="p-4 border-b border-gray-100">
            <h2 className="text-lg font-semibold text-gray-900">Recent Activity</h2>
          </div>
          <div className="p-4 space-y-4">
            {[
              { action: 'Control assessed', item: 'ECC-1.1.1', time: '2 hours ago', status: 'Compliant' },
              { action: 'Evidence uploaded', item: 'Access Policy v2.0', time: '5 hours ago', status: 'Pending' },
              { action: 'Assessment started', item: 'Q1 2026 Review', time: '1 day ago', status: 'Active' },
              { action: 'Risk identified', item: 'Third-party access', time: '2 days ago', status: 'High' },
            ].map((activity, index) => (
              <div key={index} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                <div>
                  <p className="font-medium text-gray-900">{activity.action}</p>
                  <p className="text-sm text-gray-500">{activity.item}</p>
                </div>
                <div className="text-right">
                  <span className={`inline-block px-2 py-1 text-xs rounded-full ${
                    activity.status === 'Compliant' ? 'bg-green-100 text-green-700' :
                    activity.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
                    activity.status === 'Active' ? 'bg-blue-100 text-blue-700' :
                    'bg-red-100 text-red-700'
                  }`}>
                    {activity.status}
                  </span>
                  <p className="text-xs text-gray-400 mt-1">{activity.time}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="p-4 border-b border-gray-100">
            <h2 className="text-lg font-semibold text-gray-900">Quick Actions</h2>
          </div>
          <div className="p-4 space-y-3">
            {[
              { label: 'Start Assessment', href: '/assessments/new', icon: '✅' },
              { label: 'Upload Evidence', href: '/evidence/upload', icon: '📁' },
              { label: 'View Controls', href: '/controls', icon: '🛡️' },
              { label: 'Generate Report', href: '/reports/new', icon: '📊' },
              { label: 'Manage Risks', href: '/risks', icon: '⚠️' },
            ].map((action, index) => (
              <a
                key={index}
                href={action.href}
                className="flex items-center justify-between p-3 rounded-lg border border-gray-200 hover:border-[#0E7490] hover:bg-[#0E7490]/5 transition-colors"
              >
                <span className="flex items-center space-x-3">
                  <span className="text-xl">{action.icon}</span>
                  <span className="font-medium text-gray-700">{action.label}</span>
                </span>
                <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5l7 7-7 7" />
                </svg>
              </a>
            ))}
          </div>
        </div>
      </div>

      {/* Compliance Overview */}
      <div className="mt-6 bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Compliance by Framework</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {[
            { name: 'NCA ECC', progress: 85, controls: 109, compliant: 93 },
            { name: 'SAMA CSF', progress: 72, controls: 87, compliant: 63 },
            { name: 'PDPL', progress: 90, controls: 42, compliant: 38 },
            { name: 'NCA CSCC', progress: 65, controls: 50, compliant: 33 },
          ].map((framework, index) => (
            <div key={index} className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <h3 className="font-medium text-gray-900">{framework.name}</h3>
                <span className="text-lg font-bold text-[#0E7490]">{framework.progress}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2 mb-2">
                <div
                  className="bg-[#0E7490] h-2 rounded-full transition-all"
                  style={{ width: `${framework.progress}%` }}
                />
              </div>
              <p className="text-xs text-gray-500">{framework.compliant} of {framework.controls} controls compliant</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
