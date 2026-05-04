import * as d3 from 'https://cdn.jsdelivr.net/npm/d3@7/+esm';

const CSS_VAR = (name) => getComputedStyle(document.documentElement).getPropertyValue(name).trim();

let simulation;
let nodes = [];
let links = [];
const nodeMap = new Map();
let svg;
let g;

export function initializeGraph() {
    const container = document.getElementById('network-graph');
    const width = container.clientWidth;
    const height = container.clientHeight;

    svg = d3.select(container)
        .append('svg')
        .attr('width', width)
        .attr('height', height);

    const defs = svg.append('defs');

    defs.append('marker')
        .attr('id', 'arrow-normal')
        .attr('markerWidth', 10)
        .attr('markerHeight', 10)
        .attr('refX', 22)
        .attr('refY', 5)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M0,0 L10,5 L0,10')
        .attr('fill', CSS_VAR('--border'));

    defs.append('marker')
        .attr('id', 'arrow-highlight')
        .attr('markerWidth', 10)
        .attr('markerHeight', 10)
        .attr('refX', 22)
        .attr('refY', 5)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M0,0 L10,5 L0,10')
        .attr('fill', CSS_VAR('--accent'));

    g = svg.append('g');

    svg.call(d3.zoom()
        .on('zoom', (event) => {
            g.attr('transform', event.transform);
        }));

    simulation = d3.forceSimulation(nodes)
        .force('link', d3.forceLink(links).id(d => d.id).distance(150))
        .force('charge', d3.forceManyBody().strength(-1500))
        .force('collide', d3.forceCollide(45))
        .force('center', d3.forceCenter(width / 2, height / 2));

    render();

    window.addEventListener('resize', () => {
        const newWidth = container.clientWidth;
        const newHeight = container.clientHeight;
        svg.attr('width', newWidth).attr('height', newHeight);
        simulation.force('center', d3.forceCenter(newWidth / 2, newHeight / 2));
    });
}

function addNode(ip) {
    if (!nodeMap.has(ip)) {
        const node = { id: ip, x: Math.random() * 800, y: Math.random() * 600 };
        nodes.push(node);
        nodeMap.set(ip, node);
        simulation.nodes(nodes);
    }
    return nodeMap.get(ip);
}

function addLink(source, target) {
    const existingLink = links.find(l => l.source.id === source && l.target.id === target);

    if (!existingLink) {
        links.push({ source, target, count: 1 });
        simulation.force('link').links(links);
    } else {
        existingLink.count++;
    }
}

function render() {
    let link = g.selectAll('.link').data(links, (d, i) => i);

    link.exit().remove();

    link = link.enter()
        .append('line')
        .attr('class', 'link')
        .attr('marker-end', 'url(#arrow-normal)')
        .merge(link);

    let node = g.selectAll('.node').data(nodes, d => d.id);

    const nodeEnter = node.enter()
        .append('g')
        .attr('class', 'node')
        .call(d3.drag()
            .on('start', dragstarted)
            .on('drag', dragged)
            .on('end', dragended));

    nodeEnter.append('circle')
        .attr('r', 35)
        .attr('fill', CSS_VAR('--primary'))
        .attr('opacity', 0.8);

    nodeEnter.append('text')
        .attr('text-anchor', 'middle')
        .attr('dy', '0.3em')
        .text(d => d.id);

    node = nodeEnter.merge(node);

    simulation.on('tick', () => {
        link
            .attr('x1', d => d.source.x)
            .attr('y1', d => d.source.y)
            .attr('x2', d => d.target.x)
            .attr('y2', d => d.target.y);

        node
            .attr('transform', d => `translate(${d.x},${d.y})`);
    });

    simulation.alpha(1).restart();
}

export function highlightConnection(sourceIp, destIp) {
    addNode(sourceIp);
    addNode(destIp);
    addLink(sourceIp, destIp);

    render();

    const sourceNode = d3.select(`[data-ip="${sourceIp}"]`).select('circle');
    const destNode = d3.select(`[data-ip="${destIp}"]`).select('circle');

    const highlightLink = g.selectAll('.link')
        .filter(d => d.source.id === sourceIp && d.target.id === destIp)
        .classed('highlight', true)
        .attr('marker-end', 'url(#arrow-highlight)');

    const sourceCircle = g.selectAll('circle').filter(d => d.id === sourceIp);
    const destCircle = g.selectAll('circle').filter(d => d.id === destIp);

    sourceCircle.classed('highlight', true);
    destCircle.classed('highlight', true);

    setTimeout(() => {
        highlightLink.classed('highlight', false)
            .attr('marker-end', 'url(#arrow-normal)');
        sourceCircle.classed('highlight', false);
        destCircle.classed('highlight', false);
    }, 800);
}

function dragstarted(event, d) {
    if (!event.active) simulation.alphaTarget(0.3).restart();
    d.fx = d.x;
    d.fy = d.y;
}

function dragged(event, d) {
    d.fx = event.x;
    d.fy = event.y;
}

function dragended(event, d) {
    if (!event.active) simulation.alphaTarget(0);
    d.fx = null;
    d.fy = null;
}