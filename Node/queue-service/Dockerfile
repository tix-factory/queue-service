FROM node:14

# Copies the contents of the directory the Dockerfile is in into the image.
COPY . /Application/

# NPM install runs before the image is created... I think?
RUN npm install /Application

# Set working directory for the ENTRYPOINT
WORKDIR /Application

# Run the application
ENTRYPOINT ["node", "index.js"]
