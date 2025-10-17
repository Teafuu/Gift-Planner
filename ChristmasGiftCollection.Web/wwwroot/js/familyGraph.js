let network = null;
let dotNetRef = null;
let currentMemberId = null;

export function initializeGraph(containerId, data, dotNetReference, memberId) {
    dotNetRef = dotNetReference;
    currentMemberId = memberId;

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container with id '${containerId}' not found`);
        return;
    }

    // Configure Vis.js network options for physics-based family graph with couple grouping
    const options = {
        nodes: {
            shape: 'box',
             shapeProperties: {
                interpolation: false    // 'true' for intensive zooming
             },
            size: 25,
            margin: 10,
            font: {
                size: 14,
                face: 'Arial',
                color: '#ffffff',
                bold: {
                    color: '#ffffff'
                }
            },
            borderWidth: 3,
            shadow: {
                enabled: true,
                color: 'rgba(0,0,0,0.3)',
                size: 10,
                x: 3,
                y: 3
            },
            widthConstraint: {
                minimum: 100,
                maximum: 150
            },
            chosen: {
                node: function(values, id, selected, hovering) {
                    // Preserve original colors - no color changes
                    if (hovering) {
                        // Subtle highlight on hover
                        values.borderWidth = 4;
                        values.shadowSize = 15;
                        values.shadowColor = 'rgba(0,0,0,0.5)';
                    }
                    if (selected) {
                        // Very subtle selection - just border, no color
                        values.borderWidth = 5;
                        // Keep the original border color
                        values.highlightBorderColor = values.borderColor;
                        values.hoverBorderColor = values.borderColor;
                    }
                }
            },
            // Disable color changes on select
            color: {
                highlight: {
                    border: undefined,
                    background: undefined
                },
                hover: {
                    border: undefined,
                    background: undefined
                }
            }
        },
        edges: {
            width: 2,
            shadow: true,
            smooth: {
                enabled: true,
                type: 'continuous',
                roundness: 0.5
            },
            font: {
                size: 11,
                align: 'middle',
                color: '#666666',
                strokeWidth: 0
            }
        },
        layout: {
            improvedLayout: true,
            randomSeed: 42  // Consistent layout on reload
        },
        physics: {
            enabled: true,
            stabilization: {
                enabled: true,
                iterations: 50,
                updateInterval: 50,
                fit: true
            },
            barnesHut: {
                gravitationalConstant: -2000,
                centralGravity: 0.5,
                springLength: 50,
                springConstant: 0.02,
                damping: 0.15,
                avoidOverlap: 0.5
            },
            solver: 'barnesHut',
            timestep: 0.5,
            adaptiveTimestep: true
        },
        interaction: {
            hover: true,
            tooltipDelay: 100,
            zoomView: true,
            dragView: true,
            navigationButtons: false,
            keyboard: {
                enabled: true,
                bindToWindow: false
            },
            dragNodes: true,
            zoomSpeed: 1,
            selectConnectedEdges: false
        }
    };

    // Create the network
    network = new vis.Network(container, data, options);

    // Handle node click events
    network.on('click', function (params) {
        if (params.nodes.length > 0) {
            const nodeId = params.nodes[0];
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnNodeClick', nodeId);
            }
        }
    });

    // Handle node double-click events
    network.on('doubleClick', function (params) {
        if (params.nodes.length > 0) {
            const nodeId = params.nodes[0];
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnNodeDoubleClick', nodeId);
            }
        }
    });

    // Fit the graph and stop physics after stabilization
    network.once('stabilizationIterationsDone', function () {
        network.setOptions({ physics: { enabled: true } });

        // Fit to container or focus on current member
        setTimeout(() => {
            if (currentMemberId && network.body.nodes[currentMemberId]) {
                // Focus on the current logged-in member's node
                network.focus(currentMemberId, {
                    scale: 1.5,
                    animation: {
                        duration: 1000,
                        easingFunction: 'easeInOutCubic'
                    }
                });
                console.log(`Family graph focused on member: ${currentMemberId}`);
            } else {
                // Fallback to fitting entire graph if no member ID or node not found
                network.fit({
                    animation: {
                        duration: 800,
                        easingFunction: 'easeInOutCubic'
                    },
                    padding: 50
                });
                console.log('Family graph stabilized - showing all nodes');
            }
        }, 100);

        console.log('Family graph stabilized and physics disabled');
    });

    // Optional: add visual feedback during stabilization
    network.on('stabilizationProgress', function(params) {
        const percentage = Math.round((params.iterations / params.total) * 100);
        console.log(`Building family graph: ${percentage}%`);
    });
}

export function updateGraph(data) {
    if (network) {
        network.setData(data);
    }
}

export function fitGraph() {
    if (network) {
        network.fit({
            animation: {
                duration: 500,
                easingFunction: 'easeInOutQuad'
            }
        });
    }
}

export function focusOnMember(memberId) {
    if (network && memberId) {
        currentMemberId = memberId;
        if (network.body.nodes[memberId]) {
            network.focus(memberId, {
                scale: 1.5,
                animation: {
                    duration: 1000,
                    easingFunction: 'easeInOutCubic'
                }
            });
            console.log(`Family graph focused on member: ${memberId}`);
            return true;
        } else {
            console.warn(`Member node ${memberId} not found in graph`);
            return false;
        }
    }
    return false;
}

export function dispose() {
    if (network) {
        network.destroy();
        network = null;
    }
    dotNetRef = null;
}
