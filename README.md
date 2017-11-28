# LittleReviewer
A tiny git front-end for reviewers (not developers)

This tool is designed to give non-developers a chance to test TFS pull requests before they are merged. It works by doing workspace merges into master, with no commit.

## Details

At the start of a review, master is checked-out, and the selected branch is merged in, with the options `--no-ff` and `--no-commit`.

When a review is ended, remotes are fetched, and master is reset to origin/master.
No commits or pushes are made by this tool.

## To Do:

- [ ] Check out multiple projects' pull requests at once
- [ ] Download and extract `Masters.7z` instead of folder contents if it exists
- [ ] More robust error handling & reporting
- [ ] Assume C/CodeReview if it exists
- [ ] Show last updated date remote and local
- [ ] Allow no-action?
- [ ] Write a 'last source' file in local directories to show current status
- [ ] Remove 'Cleanup Review'