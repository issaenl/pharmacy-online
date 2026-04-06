import { defineStore } from 'pinia';
import * as signalR from '@microsoft/signalr';
import api from '@/api/api';
import { useAuthStore } from './authStore';

export const useNotificationStore = defineStore('notifications', {
  state: () => ({
    notifications: [],
    unreadCount: 0,
    connection: null
  }),
  actions: {
    async fetchNotifications() {
      try {
        const res = await api.get('/Notifications/my');
        this.notifications = res.data;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      } catch (error) {
        console.error("Ошибка загрузки", error);
      }
    },

    async initSignalR() {
      const authStore = useAuthStore();
      if (!authStore.token || this.connection) return;

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5193/notificationHub", { 
          accessTokenFactory: () => authStore.token
        })
        .withAutomaticReconnect()
        .build();

      this.connection.on("ReceiveNotification", (notification) => {
        this.notifications.unshift(notification);
        this.unreadCount++;
      });

      try {
        await this.connection.start();
        console.log("Уведомления подключены!");
      } catch (err) {
        console.error("Ошибка SignalR: ", err);
      }
    },

    async stopSignalR() {
      if (this.connection) {
        await this.connection.stop();
        this.connection = null;
      }
    },

    async markAsRead() {
      try {
        await api.post('/Notifications/read-all');
        this.unreadCount = 0;
        this.notifications.forEach(n => n.isRead = true);
      } catch (error) {}
    },

    async removeNotification(id) {
      try {
        await api.delete(`/Notifications/${id}`);
        this.notifications = this.notifications.filter(n => n.id !== id);
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      } catch (error) {
        console.error("Ошибка удаления", error);
      }
    }
  }
});