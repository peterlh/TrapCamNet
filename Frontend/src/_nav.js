export default [
  {
    component: 'CNavItem',
    name: 'Dashboard',
    to: '/dashboard',
    icon: 'cil-speedometer',
    badge: {
      color: 'primary',
      text: 'NEW',
    },
  },
  {
    component: 'CNavTitle',
    name: 'Images',
  },
  {
    component: 'CNavItem',
    name: 'Listing',
    to: '/images',
    icon: 'cil-notes',
  },
  {
    component: 'CNavItem',
    name: 'Date browser',
    to: '/images/datebrowser',
    icon: 'cil-notes',
  },
  {
    component: 'CNavTitle',
    name: 'Statistics',
  },
  {
    component: 'CNavItem',
    name: 'By Animal',
    to: '/statistics/animal',
    icon: 'cil-notes',
  },
  {
    component: 'CNavItem',
    name: 'By Location',
    to: '/statistics/location',
    icon: 'cil-notes',
  },
  {
    component: 'CNavTitle',
    name: 'Settings',
  },
  {
    component: 'CNavItem',
    name: 'Cameras',
    to: '/cameras',
    icon: 'cil-camera',
  },
  {
    component: 'CNavItem',
    name: 'Locations',
    to: '/locations',
    icon: 'cil-map'
  },
  {
    component: 'CNavItem',
    name: 'Notifications',
    to: '/settings/notifications',
    icon: 'cil-bell'
  }
]
