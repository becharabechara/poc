# Use Node.js 18 Alpine for building the React app
FROM node:18-alpine AS build

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production --legacy-peer-deps

# Copy source code
COPY . .

# Set environment variable explicitly
ENV REACT_APP_API_URL=http://localhost:8080/api

# Build the application
RUN npm run build

# Use nginx to serve the built application
FROM nginx:alpine AS final

# Copy built application from build stage
COPY --from=build /app/build /usr/share/nginx/html

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]