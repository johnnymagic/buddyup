import React from 'react';
import Spinner from '../common/Spinner';

/**
 * Dashboard Stats component for admin dashboard
 * 
 * @param {Object} props - Component props
 * @param {Object} props.stats - Dashboard statistics
 */
const DashboardStats = ({ stats }) => {
  if (!stats) {
    return (
      <div className="bg-white shadow-md rounded-lg p-6">
        <div className="flex justify-center py-12">
          <Spinner size="lg" />
        </div>
      </div>
    );
  }

  // Data for user growth chart
  const userGrowthData = stats.userGrowth || [];
  
  // Data for sport popularity chart
  const sportPopularityData = stats.sportPopularity || [];
  
  // Data for match statistics
  const matchStats = stats.matchStats || {
    totalMatches: 0,
    pendingMatches: 0,
    acceptedMatches: 0,
    rejectedMatches: 0
  };
  
  // Data for verification statistics
  const verificationStats = stats.verificationStats || {
    totalVerified: 0,
    pendingVerifications: 0,
    verificationRate: 0
  };

  return (
    <div className="bg-white shadow-md rounded-lg p-6">
      <h2 className="text-2xl font-semibold mb-6">Dashboard</h2>
      
      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div className="bg-blue-50 rounded-lg p-4 shadow">
          <h3 className="text-lg font-medium text-blue-800">Total Users</h3>
          <p className="text-3xl font-bold text-blue-600 mt-2">{stats.totalUsers || 0}</p>
          <p className="text-sm text-blue-500 mt-1">
            {stats.newUsersToday > 0 && `+${stats.newUsersToday} today`}
          </p>
        </div>
        
        <div className="bg-green-50 rounded-lg p-4 shadow">
          <h3 className="text-lg font-medium text-green-800">Active Matches</h3>
          <p className="text-3xl font-bold text-green-600 mt-2">{matchStats.acceptedMatches}</p>
          <p className="text-sm text-green-500 mt-1">
            {matchStats.pendingMatches > 0 && `${matchStats.pendingMatches} pending`}
          </p>
        </div>
        
        <div className="bg-purple-50 rounded-lg p-4 shadow">
          <h3 className="text-lg font-medium text-purple-800">Verified Users</h3>
          <p className="text-3xl font-bold text-purple-600 mt-2">{verificationStats.totalVerified}</p>
          <p className="text-sm text-purple-500 mt-1">
            {`${verificationStats.verificationRate}% of total users`}
          </p>
        </div>
        
        <div className="bg-yellow-50 rounded-lg p-4 shadow">
          <h3 className="text-lg font-medium text-yellow-800">Total Sports</h3>
          <p className="text-3xl font-bold text-yellow-600 mt-2">{stats.totalSports || 0}</p>
          <p className="text-sm text-yellow-500 mt-1">
            {stats.activeSports && `${stats.activeSports} active`}
          </p>
        </div>
      </div>
      
      {/* User Growth Chart */}
      <div className="mb-8">
        <h3 className="text-xl font-semibold mb-4">User Growth</h3>
        <div className="bg-white border border-gray-200 rounded-lg p-4">
          <div className="h-64">
            {userGrowthData.length > 0 ? (
              <div className="relative h-full">
                {/* Simple bar chart representation */}
                <div className="flex h-full items-end space-x-2">
                  {userGrowthData.map((data, index) => (
                    <div key={index} className="flex-1 flex flex-col items-center">
                      <div 
                        className="w-full bg-blue-500 rounded-t"
                        style={{ 
                          height: `${(data.count / Math.max(...userGrowthData.map(d => d.count))) * 100}%`,
                          minHeight: '1px'
                        }}
                      ></div>
                      <span className="text-xs text-gray-500 mt-1">{data.period}</span>
                    </div>
                  ))}
                </div>
                
                {/* Y-axis labels */}
                <div className="absolute left-0 top-0 h-full flex flex-col justify-between text-xs text-gray-500">
                  <span>
                    {Math.max(...userGrowthData.map(d => d.count))}
                  </span>
                  <span>0</span>
                </div>
              </div>
            ) : (
              <div className="flex items-center justify-center h-full text-gray-500">
                No user growth data available
              </div>
            )}
          </div>
        </div>
      </div>
      
      {/* Two-column layout for additional stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Sport Popularity */}
        <div>
          <h3 className="text-xl font-semibold mb-4">Popular Sports</h3>
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            {sportPopularityData.length > 0 ? (
              <div className="space-y-4">
                {sportPopularityData.map((sport, index) => (
                  <div key={index}>
                    <div className="flex justify-between mb-1">
                      <span className="text-sm font-medium text-gray-700">{sport.name}</span>
                      <span className="text-sm text-gray-500">{sport.count} users</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-blue-600 h-2 rounded-full" 
                        style={{ 
                          width: `${(sport.count / sportPopularityData[0].count) * 100}%` 
                        }}
                      ></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="py-8 text-center text-gray-500">
                No sport popularity data available
              </div>
            )}
          </div>
        </div>
        
        {/* Recent Activity */}
        <div>
          <h3 className="text-xl font-semibold mb-4">Recent Activity</h3>
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            {stats.recentActivity && stats.recentActivity.length > 0 ? (
              <div className="space-y-4">
                {stats.recentActivity.map((activity, index) => (
                  <div key={index} className="flex">
                    <div className={`flex-shrink-0 h-8 w-8 rounded-full flex items-center justify-center ${
                      activity.type === 'user' ? 'bg-blue-100 text-blue-600' :
                      activity.type === 'match' ? 'bg-green-100 text-green-600' :
                      activity.type === 'verification' ? 'bg-purple-100 text-purple-600' :
                      'bg-gray-100 text-gray-600'
                    }`}>
                      {activity.type === 'user' ? 'U' :
                       activity.type === 'match' ? 'M' :
                       activity.type === 'verification' ? 'V' : 'A'}
                    </div>
                    <div className="ml-3">
                      <p className="text-sm text-gray-900">{activity.description}</p>
                      <p className="text-xs text-gray-500">{activity.time}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="py-8 text-center text-gray-500">
                No recent activity data available
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardStats;