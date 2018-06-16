import fetch from 'node-fetch';

export default class EnvironmentClient {
    constructor(private baseUrl : string,
                private apiKey: string) {   
    }

    public uploadPhoto(binary: Buffer) : Promise<any> {
        return fetch(this.baseUrl + { // Your POST endpoint
          method: 'POST',
          headers: {
            'X-API-KEY': this.apiKey,
            'Content-Type': 'image/jpeg'
          },
          body: binary
        })
        .then(result => result.json())
        .then(result => {
          console.info('Successfully uploaded binary to dashboard.');
          return result;
        })
        .catch(error => {
          console.error('An error occured while uploading a binary file.', error);
        });
    }
}