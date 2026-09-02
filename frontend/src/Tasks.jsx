import React, { useState } from 'react';
import { LayoutDashboard, CheckSquare, Users, Settings, LogOut, Bell, Search, Plus, X, CheckCircle, Clock } from 'lucide-react';
import { useNavigate, Link } from 'react-router-dom';

export default function Tasks() {
  const navigate = useNavigate();
  const [filter, setFilter] = useState('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  // Mock Tasks State (local array)
  const [tasks, setTasks] = useState([
    { id: 1, title: 'Setup .NET API with Entity Framework', description: 'Backend Development & JWT Setup', isCompleted: true },
    { id: 2, title: 'Create React Dashboard Layout', description: 'Frontend Development with Tailwind v4', isCompleted: false },
    { id: 3, title: 'Configure CORS and Routing', description: 'Connect Frontend with Backend endpoints', isCompleted: false }
  ]);

  const handleCreateTask = (e) => {
    e.preventDefault();
    const newTask = {
      id: tasks.length + 1,
      title,
      description,
      isCompleted: false
    };
    setTasks([newTask, ...tasks]);
    setTitle('');
    setDescription('');
    setIsModalOpen(false);
  };

  const toggleTaskStatus = (id) => {
    setTasks(tasks.map(t => t.id === id ? { ...t, isCompleted: !t.isCompleted } : t));
  };

  const filteredTasks = tasks.filter(task => {
    if (filter === 'completed') return task.isCompleted;
    if (filter === 'pending') return !task.isCompleted;
    return true;
  });

  return (
    <div className="flex h-screen bg-gray-50 font-sans">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6 border-b border-gray-100 flex items-center gap-2">
          <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-blue-200">T</div>
          <h1 className="text-xl font-bold text-gray-800 tracking-tight">TaskMaster</h1>
        </div>
        
        <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
          <p className="px-3 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2 mt-4">Menu</p>
          <Link to="/dashboard" className="flex items-center gap-3 p-3 text-gray-500 hover:bg-gray-50 hover:text-gray-700 rounded-xl font-medium transition-all">
            <LayoutDashboard size={20} /> Dashboard
          </Link>
          <Link to="/tasks" className="flex items-center gap-3 p-3 text-blue-700 bg-blue-50 rounded-xl font-medium transition-all">
            <CheckSquare size={20} /> My Tasks
          </Link>
          <a href="#" className="flex items-center gap-3 p-3 text-gray-500 hover:bg-gray-50 hover:text-gray-700 rounded-xl font-medium transition-all">
            <Users size={20} /> Teams
          </a>
          <a href="#" className="flex items-center gap-3 p-3 text-gray-500 hover:bg-gray-50 hover:text-gray-700 rounded-xl font-medium transition-all">
            <Settings size={20} /> Settings
          </a>
        </nav>
        
        <div className="p-4 border-t border-gray-100">
          <button onClick={() => navigate('/login')} className="flex items-center gap-3 p-3 text-red-500 hover:bg-red-50 hover:text-red-600 rounded-xl font-medium w-full transition-all">
            <LogOut size={20} /> Logout
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="bg-white h-20 border-b border-gray-200 flex items-center justify-between px-8">
          <div className="flex items-center bg-gray-100 rounded-full px-4 py-2 w-96">
            <Search size={18} className="text-gray-400" />
            <input 
              type="text" 
              placeholder="Search tasks..." 
              className="bg-transparent border-none outline-none ml-3 w-full text-sm text-gray-700 placeholder-gray-400"
            />
          </div>
          
          <div className="flex items-center gap-6">
            <button className="relative p-2 text-gray-400 hover:text-gray-600 transition-colors">
              <Bell size={22} />
              <span className="absolute top-1 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-white"></span>
            </button>
            <div className="flex items-center gap-3 border-l pl-6 border-gray-200 cursor-pointer">
              <div className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold shadow-md">
                WH
              </div>
              <div>
                <p className="text-sm font-semibold text-gray-700">Wasi Hassan</p>
                <p className="text-xs text-gray-500">Admin</p>
              </div>
            </div>
          </div>
        </header>

        {/* Tasks Content */}
        <div className="p-8 flex-1 overflow-y-auto bg-gray-50">
          <div className="mb-8 flex justify-between items-end">
            <div>
              <h2 className="text-2xl font-bold text-gray-800">Task Management 📋</h2>
              <p className="text-gray-500 mt-1">Manage, filter, and track all your assigned tasks here.</p>
            </div>
            <button 
              onClick={() => setIsModalOpen(true)}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-xl font-medium shadow-lg shadow-blue-200 transition-all flex items-center gap-2"
            >
              <Plus size={18} /> New Task
            </button>
          </div>

          {/* Filter Tabs */}
          <div className="flex gap-3 mb-6">
            <button 
              onClick={() => setFilter('all')} 
              className={`px-5 py-2 rounded-xl text-sm font-semibold transition-all ${filter === 'all' ? 'bg-blue-600 text-white shadow-md shadow-blue-100' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-100'}`}
            >
              All Tasks ({tasks.length})
            </button>
            <button 
              onClick={() => setFilter('completed')} 
              className={`px-5 py-2 rounded-xl text-sm font-semibold transition-all ${filter === 'completed' ? 'bg-green-600 text-white shadow-md shadow-green-100' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-100'}`}
            >
              Completed ({tasks.filter(t => t.isCompleted).length})
            </button>
            <button 
              onClick={() => setFilter('pending')} 
              className={`px-5 py-2 rounded-xl text-sm font-semibold transition-all ${filter === 'pending' ? 'bg-orange-500 text-white shadow-md shadow-orange-100' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-100'}`}
            >
              Pending ({tasks.filter(t => !t.isCompleted).length})
            </button>
          </div>

          {/* Tasks List */}
          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="p-6">
              {filteredTasks.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No tasks found in this category.</p>
              ) : (
                <div className="space-y-4">
                  {filteredTasks.map((task) => (
                    <div key={task.id} className="flex items-center justify-between p-4 border border-gray-100 rounded-xl hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-4">
                        <input 
                          type="checkbox" 
                          checked={task.isCompleted} 
                          onChange={() => toggleTaskStatus(task.id)}
                          className="w-5 h-5 text-blue-600 rounded-md border-gray-300 cursor-pointer" 
                        />
                        <div>
                          <p className={`font-semibold ${task.isCompleted ? 'line-through text-gray-400' : 'text-gray-800'}`}>
                            {task.title}
                          </p>
                          <p className="text-sm text-gray-500 mt-0.5">{task.description}</p>
                        </div>
                      </div>
                      <span className={`px-4 py-1 text-xs font-bold rounded-full flex items-center gap-1.5 ${task.isCompleted ? 'text-green-700 bg-green-100' : 'text-orange-700 bg-orange-100'}`}>
                        {task.isCompleted ? <CheckCircle size={14} /> : <Clock size={14} />}
                        {task.isCompleted ? 'Done' : 'In Progress'}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </main>

      {/* New Task Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 border border-gray-100">
            <div className="flex justify-between items-center mb-6">
              <h3 className="text-xl font-bold text-gray-800">Create New Task</h3>
              <button onClick={() => setIsModalOpen(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCreateTask} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Task Title</label>
                <input 
                  type="text" 
                  required 
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="e.g. Implement State Management" 
                  className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea 
                  rows="3" 
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Add details about your task..." 
                  className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm resize-none"
                ></textarea>
              </div>

              <div className="flex gap-3 pt-2">
                <button 
                  type="button" 
                  onClick={() => setIsModalOpen(false)}
                  className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 py-2.5 rounded-xl font-medium transition-all"
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-xl font-bold shadow-lg shadow-blue-200 transition-all"
                >
                  Save Task
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}