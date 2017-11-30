# LittleReviewer
A tiny file-copy front-end for reviewers.

This tool is designed to give non-developers a chance to test TFS pull requests before they are merged.

## Details

Master builds go in individual folders on a share.
Pull Request builds go in folders based on their ID.

This tool reads, presents and copies those folders around.

## To Do:

- [x] Check out multiple projects' pull requests at once
- [x] *Allow* download and extract `Archive.7z` instead of folder contents if it exists. Must have switch, as it's much slower when running on a fast network.
- [x] More robust error handling & reporting
- [x] Assume C/CodeReview if it exists
- [x] Show last updated date remote and local
- [x] Direct refresh
- [x] Allow no-action?
- [x] Write a 'last source' file in local directories to show current status
- [x] Remove 'Cleanup Review'
- [x] Help screen
