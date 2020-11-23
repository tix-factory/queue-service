FROM node:14

# Copies the contents of the directory the Dockerfile is in into the image.
COPY . /Application/

# Set working directory for the ENTRYPOINT
WORKDIR /Application

# Install node modules
RUN npm install

# Run the application
ENTRYPOINT ["node", "index.js"]
