mkdocscombine -o docs/RuntimeEditor.pd
pandoc --toc -f markdown+grid_tables+table_captions -o docs/RuntimeEditor.pdf docs/RuntimeEditor.pd 
cd docs
del RuntimeEditor.pd 
