# Environment Dashboard [![Build Status](https://travis-ci.org/karl-sjogren/environment-dashboard.svg?branch=develop)](https://travis-ci.org/karl-sjogren/environment-dashboard)

A small dashboard I built to monitor the temperature and such out in my cabin.

It is built to store statistics in mongodb and snapshot images in an S3 bucket.

It can preferably be deployed to Heroku.

## Setup

Required environment variables or user secrets.

- MONGODB_URI
- AWS_ACCESS_KEY
- AWS_SECRET_KEY
- AWS_BUCKET_NAME
- AWS_REGION