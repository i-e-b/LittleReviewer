# LittleReviewer
A tiny git front-end for reviewers (not developers)

This tool is designed to give non-developers a chance to test TFS pull requests before they are merged. It works by doing workspace merges into master, with no commit.

## Details

At the start of a review, master is checked-out, and the selected branch is merged in, with the options `--no-ff` and `--no-commit`.

When a review is ended, remotes are fetched, and master is reset to origin/master.
No commits or pushes are made by this tool.
