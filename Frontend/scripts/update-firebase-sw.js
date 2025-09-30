// Script to update Firebase service worker with environment variables
const fs = require('fs');
const path = require('path');
require('dotenv').config();

// Path to the service worker file
const swPath = path.join(__dirname, '../public/firebase-messaging-sw.js');

console.log('Updating Firebase service worker with environment variables...');

try {
  // Read the service worker file
  let swContent = fs.readFileSync(swPath, 'utf8');
  
  // Replace placeholders with actual values
  swContent = swContent
    .replace('FIREBASE_API_KEY', process.env.VITE_FIREBASE_API_KEY || '')
    .replace('FIREBASE_AUTH_DOMAIN', process.env.VITE_FIREBASE_AUTH_DOMAIN || '')
    .replace('FIREBASE_PROJECT_ID', process.env.VITE_FIREBASE_PROJECT_ID || '')
    .replace('FIREBASE_STORAGE_BUCKET', process.env.VITE_FIREBASE_STORAGE_BUCKET || '')
    .replace('FIREBASE_MESSAGING_SENDER_ID', process.env.VITE_FIREBASE_MESSAGING_SENDER_ID || '')
    .replace('FIREBASE_APP_ID', process.env.VITE_FIREBASE_APP_ID || '');
  
  // Write the updated content back to the file
  fs.writeFileSync(swPath, swContent);
  
  console.log('Firebase service worker updated successfully!');
} catch (error) {
  console.error('Error updating Firebase service worker:', error);
  process.exit(1);
}
