# Environment Dashboard [![Build Status](https://travis-ci.org/karl-sjogren/environment-dashboard.svg?branch=develop)](https://travis-ci.org/karl-sjogren/environment-dashboard)

A small dashboard I built to monitor the temperature and such out in my cabin.

It is built to store statistics in mongodb and snapshot images in an S3 bucket.

It can preferably be deployed to Heroku using the following buildpack.

https://github.com/karl-sjogren/dotnetcore-buildpack/tree/environment-dashboard

## API setup

Required environment variables or user secrets.

- MONGODB_URI
- AWS_ACCESS_KEY
- AWS_SECRET_KEY
- AWS_BUCKET_NAME
- AWS_REGION

## Node setup

This has been tested using Raspbian June 2018.

### Install the BCM2835 driver

Gonna write this later.

### Checkout scripts

The easiest way to get the node scripts is to simply clone this whole repository.

```
git clone https://github.com/karl-sjogren/environment-dashboard.git envdashboard
```

To get a smaller (and faster) checkout a sparse checkout can be used by running
the script below.

```
git init envdashboard
cd envdashboard
git remote add origin https://github.com/karl-sjogren/environment-dashboard.git
git config core.sparsecheckout true
echo "src/node-scripts/" >> .git/info/sparse-checkout
git pull origin master
```

To get updates for the node scripts later just run `git pull origin master` in the envdashboard folder.

### Add cronjobs

Gonna write this later.

## Attributions

Default logo from https://pixabay.com/sv/milj%C3%B6-friendly-eco-natur-3420052/,
licensed under Creative Commons Zero.

Sample image from https://pixabay.com/sv/giraff-zebra-afrika-safari-djurliv-1082168/,
licensed under Creative Commons Zero.

YR weather symbols from https://github.com/YR/weather-symbols, licensed under MIT license.

Also see [nodes-cripts/packages.json](src/node-scripts/package.json) and
[webapp/packages.json](src/webapp/package.json) for npm packages. See
[EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj](src/EnvironmentDashboard.Api/EnvironmentDashboard.Api.csproj)
for nuget packages.